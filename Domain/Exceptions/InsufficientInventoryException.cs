namespace Domain.Exceptions;

public sealed class InsufficientInventoryException : DomainException
{
    public InsufficientInventoryException(
        string inventoryItemId,
        string storeId,
        string productId,
        string? productVariantId,
        int requestedQuantity,
        int availableQuantity)
        : base(
            $"Insufficient inventory to reserve the requested quantity. Requested quantity: {requestedQuantity}, available quantity: {availableQuantity}, inventory item id: {inventoryItemId}, store id: {storeId}, product id: {productId}, product variant id: {productVariantId ?? "N/A"}.")
    {
        InventoryItemId = inventoryItemId;
        StoreId = storeId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }

    public string InventoryItemId { get; }
    public string StoreId { get; }
    public string ProductId { get; }
    public string? ProductVariantId { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }
}
