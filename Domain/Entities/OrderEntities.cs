namespace Domain.Entities;

public sealed class Order : BaseAuditableEntity
{
    public string OrderNumber { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public User User { get; private set; } = default!;
    public string CustomerEmail { get; private set; } = string.Empty;
    public double CustomerLatitude { get; private set; }
    public double CustomerLongitude { get; private set; }
    public string? AllocatedStoreId { get; private set; }
    public Store? AllocatedStore { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;
    public decimal TotalAmount { get; private set; }
    public ICollection<OrderItem> Items { get; private set; } = [];

    private Order()
    {
    }

    public Order(string userId, string customerEmail, double customerLatitude, double customerLongitude)
    {
        OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        UserId = userId;
        CustomerEmail = customerEmail;
        CustomerLatitude = customerLatitude;
        CustomerLongitude = customerLongitude;
    }

    public void AllocateToStore(string storeId)
    {
        AllocatedStoreId = storeId;
        Status = OrderStatus.Allocated;
    }

    public void SetCustomerLocation(double customerLatitude, double customerLongitude)
    {
        CustomerLatitude = customerLatitude;
        CustomerLongitude = customerLongitude;
    }

    public void SetCustomerEmail(string customerEmail)
    {
        CustomerEmail = customerEmail;
    }

    public void AddItem(Product product, ProductVariant? variant, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Order item quantity must be greater than zero.");
        }

        var unitPrice = variant?.PriceOverride ?? product.BasePrice;
        var item = new OrderItem(Id, product.Id, variant?.Id, product.Name, variant?.Name, quantity, unitPrice);
        Items.Add(item);
        TotalAmount += item.LineTotal;
    }

    public void UpdateItemQuantity(string orderItemId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Order item quantity must be greater than zero.");
        }

        var item = Items.FirstOrDefault(orderItem => orderItem.Id == orderItemId)
            ?? throw new InvalidOperationException($"Order item '{orderItemId}' was not found.");

        TotalAmount -= item.LineTotal;
        item.UpdateQuantity(quantity);
        TotalAmount += item.LineTotal;
    }

    public void RemoveItem(string orderItemId)
    {
        var item = Items.FirstOrDefault(orderItem => orderItem.Id == orderItemId)
            ?? throw new InvalidOperationException($"Order item '{orderItemId}' was not found.");

        Items.Remove(item);
        TotalAmount -= item.LineTotal;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Delivered orders cannot be cancelled.");
        }

        Status = OrderStatus.Cancelled;
    }

    public void Checkout(string storeId)
    {
        if (Items.Count == 0)
        {
            throw new InvalidOperationException("Cannot checkout an empty cart.");
        }

        AllocateToStore(storeId);
    }
}

public sealed class OrderItem : BaseAuditableEntity
{
    public string OrderId { get; private set; } = string.Empty;
    public Order Order { get; private set; } = default!;
    public string ProductId { get; private set; } = string.Empty;
    public string? ProductVariantId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string? VariantName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;

    private OrderItem()
    {
    }

    public OrderItem(
        string orderId,
        string productId,
        string? productVariantId,
        string productName,
        string? variantName,
        int quantity,
        decimal unitPrice)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        ProductName = productName;
        VariantName = variantName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Order item quantity must be greater than zero.");
        }

        Quantity = quantity;
    }
}
