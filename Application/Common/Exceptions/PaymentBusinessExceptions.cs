namespace Application.Common.Exceptions;

public sealed class PaymentNotFoundException(string paymentId)
    : BusinessLogicException($"Payment '{paymentId}' was not found.", 404)
{
    public string PaymentId { get; } = paymentId;
}

public sealed class DuplicatePaymentProviderEventException(string providerEventId)
    : BusinessLogicException($"Payment provider event '{providerEventId}' has already been recorded.", 409)
{
    public string ProviderEventId { get; } = providerEventId;
}

public sealed class InvalidPaymentOperationException(string message)
    : BusinessLogicException(message, 400);
