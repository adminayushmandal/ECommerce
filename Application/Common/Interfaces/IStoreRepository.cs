using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IStoreRepository
{
    Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken cancellationToken);
    Task<Store?> GetByIdAsync(string storeId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string storeId, CancellationToken cancellationToken);
}
