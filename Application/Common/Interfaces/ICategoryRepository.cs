using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(string categoryId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string categoryId, CancellationToken cancellationToken);
}
