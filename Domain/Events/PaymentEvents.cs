namespace Domain.Events;

public sealed record PaymentCreatedEvent(Payment Payment) : BaseEvent;

public sealed record PaymentRequiresActionEvent(Payment Payment) : BaseEvent;

public sealed record PaymentCapturedEvent(Payment Payment) : BaseEvent;

public sealed record PaymentFailedEvent(Payment Payment) : BaseEvent;

public sealed record PaymentCancelledEvent(Payment Payment) : BaseEvent;

public sealed record PaymentRefundRecordedEvent(Payment Payment, decimal RefundAmount) : BaseEvent;

public sealed record PaymentProviderEventRecordedEvent(Payment Payment, PaymentEvent ProviderEvent) : BaseEvent;
