namespace Application.Common.Exceptions;

public sealed class CurrentUserUnavailableException()
    : BusinessLogicException("The current user context is not available.", 401);

public sealed class OrderNotFoundException(string orderId)
    : BusinessLogicException($"Order '{orderId}' was not found.", 404)
{
    public string OrderId { get; } = orderId;
}

public sealed class CartNotFoundException()
    : BusinessLogicException("The cart for the current user was not found.", 404);

public sealed class OrderItemNotFoundException(string orderItemId)
    : BusinessLogicException($"Order item '{orderItemId}' was not found.", 404)
{
    public string OrderItemId { get; } = orderItemId;
}

public sealed class EmptyCartCheckoutException()
    : BusinessLogicException("The cart is empty and cannot be checked out.", 400);

public sealed class InvalidOrderOperationException(string message)
    : BusinessLogicException(message, 400);

public sealed class UnableToAllocateStoreException(string message)
    : BusinessLogicException(message, 409);

public sealed class StoreNotFoundException(string storeId)
    : BusinessLogicException($"Store '{storeId}' was not found.", 404)
{
    public string StoreId { get; } = storeId;
}
