using Domain.Enums;

namespace Application.Common.Models;

public sealed record PayPalCreateOrderDto(
    string PaymentId,
    string OrderId,
    string OrderNumber,
    string PayPalOrderId,
    string ApprovalUrl,
    decimal Amount,
    string CurrencyCode);

public sealed record PayPalCaptureDto(
    string PaymentId,
    string OrderId,
    string OrderNumber,
    string PayPalOrderId,
    string? PayPalCaptureId,
    PaymentStatus PaymentStatus,
    OrderStatus OrderStatus,
    decimal Amount,
    string CurrencyCode);

public sealed record PayPalClientConfigDto(
    string ClientId,
    string CurrencyCode,
    string Intent);
