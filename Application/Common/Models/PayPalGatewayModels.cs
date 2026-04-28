namespace Application.Common.Models;

public sealed record PayPalCreateOrderRequest(
    string OrderId,
    string OrderNumber,
    string PaymentId,
    decimal Amount,
    string CurrencyCode,
    string ReturnUrl,
    string CancelUrl,
    string IdempotencyKey);

public sealed record PayPalCreateOrderResult(
    string PayPalOrderId,
    string ApprovalUrl,
    string Status);

public sealed record PayPalCaptureOrderResult(
    string PayPalOrderId,
    string? PayPalCaptureId,
    string Status);
