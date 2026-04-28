using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IPayPalCheckoutGateway
{
    string ClientId { get; }
    string CurrencyCode { get; }
    Task<PayPalCreateOrderResult> CreateOrderAsync(PayPalCreateOrderRequest request, CancellationToken cancellationToken);
    Task<PayPalCaptureOrderResult> CaptureOrderAsync(string payPalOrderId, string idempotencyKey, CancellationToken cancellationToken);
}
