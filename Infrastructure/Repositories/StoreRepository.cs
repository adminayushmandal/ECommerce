using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class StoreRepository(ApplicationDbContext dbContext) : IStoreRepository
{
    public async Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Stores
            .AsNoTracking()
            .OrderBy(store => store.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Store?> GetByIdAsync(string storeId, CancellationToken cancellationToken)
    {
        return dbContext.Stores.FirstOrDefaultAsync(store => store.Id == storeId, cancellationToken);
    }

    public Task<bool> ExistsAsync(string storeId, CancellationToken cancellationToken)
    {
        return dbContext.Stores.AnyAsync(store => store.Id == storeId, cancellationToken);
    }
}
