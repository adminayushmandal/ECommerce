namespace Domain.Enums;

public enum PaymentStatus
{
    Pending = 0,
    RequiresAction = 1,
    Captured = 2,
    Failed = 3,
    Cancelled = 4,
    PartiallyRefunded = 5,
    Refunded = 6
}
