namespace Domain.Entities;

public sealed class Payment : BaseAuditableEntity
{
    public string OrderId { get; private set; } = string.Empty;
    public Order Order { get; private set; } = default!;
    public PaymentProvider Provider { get; private set; } = PaymentProvider.Unknown;
    public string? ProviderOrderId { get; private set; }
    public string? ProviderCaptureId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? FailureReason { get; private set; }
    public DateTimeOffset? CapturedAt { get; private set; }
    public decimal RefundedAmount { get; private set; }
    public ICollection<PaymentEvent> Events { get; private set; } = [];

    private Payment()
    {
    }

    public Payment(string orderId, PaymentProvider provider, decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new ArgumentException("Order id is required.", nameof(orderId));
        }

        if (provider == PaymentProvider.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(provider), provider, "Payment provider is required.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Payment amount must be greater than zero.");
        }

        OrderId = orderId;
        Provider = provider;
        Amount = amount;
        Currency = NormalizeCurrency(currency);

        AddDomainEvent(new PaymentCreatedEvent(this));
    }

    public void MarkRequiresAction(string providerOrderId)
    {
        ProviderOrderId = ValidateProviderReference(providerOrderId, nameof(providerOrderId));
        Status = PaymentStatus.RequiresAction;
        FailureReason = null;

        AddDomainEvent(new PaymentRequiresActionEvent(this));
    }

    public void MarkCaptured(string providerCaptureId, DateTimeOffset capturedAt)
    {
        ProviderCaptureId = ValidateProviderReference(providerCaptureId, nameof(providerCaptureId));
        CapturedAt = capturedAt;
        Status = PaymentStatus.Captured;
        FailureReason = null;

        AddDomainEvent(new PaymentCapturedEvent(this));
    }

    public void MarkFailed(string failureReason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = string.IsNullOrWhiteSpace(failureReason)
            ? "Payment failed."
            : failureReason.Trim();

        AddDomainEvent(new PaymentFailedEvent(this));
    }

    public void MarkCancelled(string? reason = null)
    {
        Status = PaymentStatus.Cancelled;
        FailureReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

        AddDomainEvent(new PaymentCancelledEvent(this));
    }

    public void RecordRefund(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Refund amount must be greater than zero.");
        }

        if (RefundedAmount + amount > Amount)
        {
            throw new InvalidOperationException("Refund amount cannot exceed the payment amount.");
        }

        RefundedAmount += amount;
        Status = RefundedAmount == Amount
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;

        AddDomainEvent(new PaymentRefundRecordedEvent(this, amount));
    }

    public PaymentEvent AddEvent(
        string providerEventId,
        string eventType,
        DateTimeOffset occurredAt,
        string? rawPayload)
    {
        var paymentEvent = new PaymentEvent(Id, providerEventId, eventType, occurredAt, rawPayload);
        Events.Add(paymentEvent);
        AddDomainEvent(new PaymentProviderEventRecordedEvent(this, paymentEvent));
        return paymentEvent;
    }

    private static string NormalizeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Payment currency is required.", nameof(currency));
        }

        var normalized = currency.Trim().ToUpperInvariant();

        if (normalized.Length != 3)
        {
            throw new ArgumentException("Payment currency must be a three-letter ISO code.", nameof(currency));
        }

        return normalized;
    }

    private static string ValidateProviderReference(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Provider reference is required.", parameterName);
        }

        return value.Trim();
    }
}

public sealed class PaymentEvent : BaseAuditableEntity
{
    public string PaymentId { get; private set; } = string.Empty;
    public Payment Payment { get; private set; } = default!;
    public string ProviderEventId { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? RawPayload { get; private set; }

    private PaymentEvent()
    {
    }

    public PaymentEvent(
        string paymentId,
        string providerEventId,
        string eventType,
        DateTimeOffset occurredAt,
        string? rawPayload)
    {
        if (string.IsNullOrWhiteSpace(paymentId))
        {
            throw new ArgumentException("Payment id is required.", nameof(paymentId));
        }

        if (string.IsNullOrWhiteSpace(providerEventId))
        {
            throw new ArgumentException("Provider event id is required.", nameof(providerEventId));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Payment event type is required.", nameof(eventType));
        }

        PaymentId = paymentId;
        ProviderEventId = providerEventId.Trim();
        EventType = eventType.Trim();
        OccurredAt = occurredAt;
        RawPayload = string.IsNullOrWhiteSpace(rawPayload) ? null : rawPayload;
    }

    public void MarkProcessed(DateTimeOffset processedAt)
    {
        ProcessedAt = processedAt;
    }
}
