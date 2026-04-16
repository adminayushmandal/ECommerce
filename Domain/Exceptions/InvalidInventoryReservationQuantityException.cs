namespace Domain.Exceptions;

public sealed class InvalidInventoryReservationQuantityException : DomainException
{
    public InvalidInventoryReservationQuantityException(int requestedQuantity)
        : base($"Reserved quantity must be greater than zero. Requested quantity: {requestedQuantity}.")
    {
        RequestedQuantity = requestedQuantity;
    }

    public int RequestedQuantity { get; }
}
