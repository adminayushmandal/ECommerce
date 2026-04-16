namespace Domain.Entities;

public sealed class Store : BaseAuditableEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string AddressLine1 { get; private set; } = string.Empty;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<InventoryItem> InventoryItems { get; private set; } = [];
    public ICollection<Order> Orders { get; private set; } = [];

    private Store()
    {
    }

    public Store(
        string code,
        string name,
        string addressLine1,
        string city,
        string state,
        string country,
        string postalCode,
        double latitude,
        double longitude,
        string? addressLine2 = null)
    {
        Code = code;
        Name = name;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        Country = country;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
    }
}

public sealed class InventoryItem : BaseAuditableEntity
{
    public string StoreId { get; private set; } = string.Empty;
    public Store Store { get; private set; } = default!;
    public string ProductId { get; private set; } = string.Empty;
    public Product Product { get; private set; } = default!;
    public string? ProductVariantId { get; private set; }
    public ProductVariant? ProductVariant { get; private set; }
    public int QuantityOnHand { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int ReorderThreshold { get; private set; }
    public int AvailableQuantity => QuantityOnHand - ReservedQuantity;

    private InventoryItem()
    {
    }

    public InventoryItem(string storeId, string productId, string? productVariantId, int quantityOnHand, int reorderThreshold)
    {
        StoreId = storeId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        QuantityOnHand = quantityOnHand;
        ReorderThreshold = reorderThreshold;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidInventoryReservationQuantityException(quantity);
        }

        if (AvailableQuantity < quantity)
        {
            throw new InsufficientInventoryException(
                Id,
                StoreId,
                ProductId,
                ProductVariantId,
                quantity,
                AvailableQuantity);
        }

        ReservedQuantity += quantity;
    }
}
