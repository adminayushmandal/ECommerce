namespace Domain.Enums;

public enum OrderStatus
{
    Draft = 0,
    PendingPayment = 1,
    Allocated = 2,
    Packed = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6
}
