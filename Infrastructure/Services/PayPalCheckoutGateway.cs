using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.Extensions.Options;
using PaypalServerSdk.Standard;
using PaypalServerSdk.Standard.Authentication;
using PaypalServerSdk.Standard.Exceptions;
using PaypalServerSdk.Standard.Http.Response;
using PaypalServerSdk.Standard.Models;
using System.Globalization;
using PayPalEnvironment = PaypalServerSdk.Standard.Environment;

namespace Infrastructure.Services;

internal sealed class PayPalCheckoutGateway(
    PaypalServerSdkClient payPalClient,
    IOptions<PayPalSdkOptions> options) : IPayPalCheckoutGateway
{
    private readonly PayPalSdkOptions options = options.Value;

    public string ClientId => options.ClientId;
    public string CurrencyCode => options.CurrencyCode.ToUpperInvariant();

    public async Task<PayPalCreateOrderResult> CreateOrderAsync(
        PayPalCreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var orderRequest = new OrderRequest
        {
            Intent = CheckoutPaymentIntent.Capture,
            PurchaseUnits =
            [
                new PurchaseUnitRequest
                {
                    ReferenceId = request.OrderId,
                    CustomId = request.PaymentId,
                    InvoiceId = request.OrderNumber,
                    Description = $"Order {request.OrderNumber}",
                    Amount = new AmountWithBreakdown
                    {
                        CurrencyCode = request.CurrencyCode,
                        MValue = FormatAmount(request.Amount)
                    }
                }
            ],
            PaymentSource = new PaymentSource
            {
                Paypal = new PaypalWallet
                {
                    ExperienceContext = new PaypalWalletExperienceContext
                    {
                        BrandName = options.BrandName,
                        Locale = options.Locale,
                        ShippingPreference = PaypalWalletContextShippingPreference.NoShipping,
                        UserAction = PaypalExperienceUserAction.PayNow,
                        PaymentMethodPreference = PayeePaymentMethodPreference.ImmediatePaymentRequired,
                        ReturnUrl = request.ReturnUrl,
                        CancelUrl = request.CancelUrl
                    }
                }
            }
        };

        var response = await CreatePayPalOrderAsync(orderRequest, request.IdempotencyKey, request.CurrencyCode, cancellationToken);

        var order = response.Data;
        var payPalOrderId = order?.Id;
        var approvalUrl = GetPayerApprovalUrl(order?.Links);

        if (string.IsNullOrWhiteSpace(payPalOrderId) || string.IsNullOrWhiteSpace(approvalUrl))
        {
            throw new InvalidPaymentOperationException(
                $"PayPal did not return a payer approval URL for the checkout order. Returned status: {order?.Status?.ToString() ?? "Unknown"}. Returned links: {FormatLinkRels(order?.Links)}.");
        }

        return new PayPalCreateOrderResult(payPalOrderId, approvalUrl, order?.Status?.ToString() ?? "Created");
    }

    public async Task<PayPalCaptureOrderResult> CaptureOrderAsync(
        string payPalOrderId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        ApiResponse<Order> response;

        try
        {
            response = await payPalClient.OrdersController.CaptureOrderAsync(
                new CaptureOrderInput
                {
                    Id = payPalOrderId,
                    ContentType = "application/json",
                    Prefer = "return=representation",
                    PaypalRequestId = idempotencyKey,
                    Body = new OrderCaptureRequest()
                },
                cancellationToken);
        }
        catch (ErrorException exception)
        {
            throw CreateInvalidPaymentOperationException("capture the checkout order", exception);
        }

        var order = response.Data;
        var capture = order?.PurchaseUnits?
            .SelectMany(purchaseUnit => purchaseUnit.Payments?.Captures ?? [])
            .FirstOrDefault();

        return new PayPalCaptureOrderResult(
            order?.Id ?? payPalOrderId,
            capture?.Id,
            capture?.Status?.ToString() ?? order?.Status?.ToString() ?? "Unknown");
    }

    public static PaypalServerSdkClient CreateClient(PayPalSdkOptions options)
    {
        var authModel = new ClientCredentialsAuthModel.Builder(options.ClientId, options.ClientSecret)
            .Build();

        return new PaypalServerSdkClient.Builder()
            .Environment(ParseEnvironment(options.Environment))
            .ClientCredentialsAuth(authModel)
            .Build();
    }

    private async Task<ApiResponse<Order>> CreatePayPalOrderAsync(
        OrderRequest orderRequest,
        string idempotencyKey,
        string currencyCode,
        CancellationToken cancellationToken)
    {
        try
        {
            return await payPalClient.OrdersController.CreateOrderAsync(
                new CreateOrderInput
                {
                    ContentType = "application/json",
                    Body = orderRequest,
                    Prefer = "return=representation",
                    PaypalRequestId = idempotencyKey
                },
                cancellationToken);
        }
        catch (ErrorException exception)
        {
            throw CreateInvalidPaymentOperationException("create the checkout order", exception, currencyCode);
        }
    }

    private static InvalidPaymentOperationException CreateInvalidPaymentOperationException(
        string operation,
        ErrorException exception,
        string? currencyCode = null)
    {
        var details = exception.Details?
            .Select(FormatErrorDetail)
            .Where(detail => !string.IsNullOrWhiteSpace(detail))
            .ToArray();

        var reason = details is { Length: > 0 }
            ? string.Join("; ", details)
            : exception.Message;

        if (IsCurrencyError(exception) && string.Equals(currencyCode, "INR", StringComparison.OrdinalIgnoreCase))
        {
            reason = "PayPal does not currently support INR checkout for India domestic payments. Use a supported cross-border currency such as USD for PayPal sandbox orders.";
        }

        var debugId = string.IsNullOrWhiteSpace(exception.DebugId)
            ? string.Empty
            : $" PayPal debug_id: {exception.DebugId}.";

        return new InvalidPaymentOperationException($"PayPal could not {operation}: {reason}.{debugId}");
    }

    private static bool IsCurrencyError(ErrorException exception)
    {
        return exception.Details?.Any(detail =>
            string.Equals(detail.Issue, "CURRENCY_NOT_SUPPORTED", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(detail.Issue, "CURRENCY_NOT_ALLOWED", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(detail.Issue, "UNSUPPORTED_PAYEE_CURRENCY", StringComparison.OrdinalIgnoreCase)) == true;
    }

    private static string FormatErrorDetail(ErrorDetails detail)
    {
        if (string.IsNullOrWhiteSpace(detail.Issue))
        {
            return detail.Description ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(detail.Description))
        {
            return detail.Issue;
        }

        return $"{detail.Issue}: {detail.Description}";
    }

    private static string? GetPayerApprovalUrl(IEnumerable<LinkDescription>? links)
    {
        return links?
            .FirstOrDefault(link =>
                string.Equals(link.Rel, "approve", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(link.Rel, "payer-action", StringComparison.OrdinalIgnoreCase))
            ?.Href;
    }

    private static string FormatLinkRels(IEnumerable<LinkDescription>? links)
    {
        var rels = links?
            .Select(link => string.IsNullOrWhiteSpace(link.Rel) ? "(missing rel)" : link.Rel)
            .ToArray();

        return rels is { Length: > 0 }
            ? string.Join(", ", rels)
            : "none";
    }

    private static PayPalEnvironment ParseEnvironment(string environment)
    {
        return string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(environment, "Live", StringComparison.OrdinalIgnoreCase)
            ? PayPalEnvironment.Production
            : PayPalEnvironment.Sandbox;
    }

    private static string FormatAmount(decimal amount)
    {
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero).ToString("0.00", CultureInfo.InvariantCulture);
    }
}
