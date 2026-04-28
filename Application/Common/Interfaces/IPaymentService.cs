using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IPaymentService
{
    Task<Payment?> GetByIdAsync(string paymentId, CancellationToken cancellationToken);
    Task<Payment?> GetByProviderOrderIdAsync(PaymentProvider provider, string providerOrderId, CancellationToken cancellationToken);
    Task<bool> HasProviderEventAsync(string providerEventId, CancellationToken cancellationToken);
    Task<Payment> CreatePaymentAsync(string orderId, PaymentProvider provider, decimal amount, string currency, CancellationToken cancellationToken);
    Task<Payment> MarkRequiresActionAsync(string paymentId, string providerOrderId, CancellationToken cancellationToken);
    Task<Payment> MarkCapturedAsync(string paymentId, string providerCaptureId, DateTimeOffset? capturedAt, CancellationToken cancellationToken);
    Task<Payment> MarkFailedAsync(string paymentId, string failureReason, CancellationToken cancellationToken);
    Task<Payment> MarkCancelledAsync(string paymentId, string? reason, CancellationToken cancellationToken);
    Task<Payment> RecordRefundAsync(string paymentId, decimal amount, CancellationToken cancellationToken);
    Task<PaymentEvent> RecordProviderEventAsync(
        string paymentId,
        string providerEventId,
        string eventType,
        DateTimeOffset occurredAt,
        string? rawPayload,
        CancellationToken cancellationToken);
}
