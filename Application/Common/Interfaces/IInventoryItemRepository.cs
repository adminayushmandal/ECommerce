using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IInventoryItemRepository
{
    Task<IReadOnlyList<InventoryItem>> GetByStoreIdAsync(string storeId, CancellationToken cancellationToken);
    Task<IReadOnlyList<InventoryItem>> GetByProductIdAsync(string productId, CancellationToken cancellationToken);
    Task<InventoryItem?> GetByIdAsync(string inventoryItemId, CancellationToken cancellationToken);
}
