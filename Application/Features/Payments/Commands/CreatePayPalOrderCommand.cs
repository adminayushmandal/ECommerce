using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Enums;
using MediatR;

namespace Application.Features.Payments.Commands;

public sealed record CreatePayPalOrderCommand(
    string? StoreId,
    string CustomerEmail,
    double CustomerLatitude,
    double CustomerLongitude,
    string ReturnUrl,
    string CancelUrl) : IRequest<PayPalCreateOrderDto>;

public sealed class CreatePayPalOrderCommandHandler(
    IOrderRepository orderRepository,
    IPaymentService paymentService,
    IPayPalCheckoutGateway payPalGateway,
    OrderCheckoutService checkoutService,
    IApplicationDbContext applicationDbContext,
    IUser user)
    : IRequestHandler<CreatePayPalOrderCommand, PayPalCreateOrderDto>
{
    public async Task<PayPalCreateOrderDto> Handle(CreatePayPalOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var cart = await orderRepository.GetDraftByUserIdAsync(userId, cancellationToken)
            ?? throw new CartNotFoundException();

        if (cart.Items.Count == 0)
        {
            throw new EmptyCartCheckoutException();
        }

        var returnUrl = ValidateAbsoluteUrl(request.ReturnUrl, nameof(request.ReturnUrl));
        var cancelUrl = ValidateAbsoluteUrl(request.CancelUrl, nameof(request.CancelUrl));

        cart.SetCustomerEmail(request.CustomerEmail);
        cart.SetCustomerLocation(request.CustomerLatitude, request.CustomerLongitude);

        var allocatedStore = await checkoutService.ResolveAndReserveInventoryAsync(
            cart,
            request.StoreId,
            request.CustomerLatitude,
            request.CustomerLongitude,
            cancellationToken);

        cart.BeginPayment(allocatedStore.Id);

        var payment = await paymentService.CreatePaymentAsync(
            cart.Id,
            PaymentProvider.PayPal,
            cart.TotalAmount,
            payPalGateway.CurrencyCode,
            cancellationToken);

        var payPalOrder = await payPalGateway.CreateOrderAsync(
            new PayPalCreateOrderRequest(
                cart.Id,
                cart.OrderNumber,
                payment.Id,
                payment.Amount,
                payment.Currency,
                returnUrl,
                cancelUrl,
                payment.Id),
            cancellationToken);

        payment.MarkRequiresAction(payPalOrder.PayPalOrderId);

        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return new PayPalCreateOrderDto(
            payment.Id,
            cart.Id,
            cart.OrderNumber,
            payPalOrder.PayPalOrderId,
            payPalOrder.ApprovalUrl,
            payment.Amount,
            payment.Currency);
    }

    private static string ValidateAbsoluteUrl(string value, string parameterName)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidPaymentOperationException($"{parameterName} must be an absolute HTTP URL.");
        }

        return uri.ToString();
    }
}
