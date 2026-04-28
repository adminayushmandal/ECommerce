using Domain.Enums;

namespace Application.Common.Models;

public sealed record StoreManagerPaymentDto(
    string PaymentId,
    string OrderId,
    string OrderNumber,
    string CustomerEmail,
    string? AllocatedStoreId,
    string? AllocatedStoreName,
    PaymentProvider Provider,
    string? ProviderOrderId,
    string? ProviderCaptureId,
    PaymentStatus PaymentStatus,
    OrderStatus OrderStatus,
    decimal Amount,
    string Currency,
    decimal RefundedAmount,
    string? FailureReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CapturedAt);
