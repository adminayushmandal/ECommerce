using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class InventoryItemRepository(ApplicationDbContext dbContext) : IInventoryItemRepository
{
    public async Task<IReadOnlyList<InventoryItem>> GetByStoreIdAsync(string storeId, CancellationToken cancellationToken)
    {
        return await dbContext.InventoryItems
            .AsNoTracking()
            .Include(inventoryItem => inventoryItem.Product)
            .Include(inventoryItem => inventoryItem.ProductVariant)
            .Where(inventoryItem => inventoryItem.StoreId == storeId)
            .OrderBy(inventoryItem => inventoryItem.Product.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetByProductIdAsync(string productId, CancellationToken cancellationToken)
    {
        return await dbContext.InventoryItems
            .AsNoTracking()
            .Include(inventoryItem => inventoryItem.Store)
            .Include(inventoryItem => inventoryItem.ProductVariant)
            .Where(inventoryItem => inventoryItem.ProductId == productId)
            .OrderBy(inventoryItem => inventoryItem.Store.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<InventoryItem?> GetByIdAsync(string inventoryItemId, CancellationToken cancellationToken)
    {
        return dbContext.InventoryItems
            .Include(inventoryItem => inventoryItem.Store)
            .Include(inventoryItem => inventoryItem.Product)
            .Include(inventoryItem => inventoryItem.ProductVariant)
            .FirstOrDefaultAsync(inventoryItem => inventoryItem.Id == inventoryItemId, cancellationToken);
    }
}
