using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetListAsync(CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(string productId, CancellationToken cancellationToken);
    Task<bool> ExistsBySkuAsync(string sku, string? excludeProductId, CancellationToken cancellationToken);
    Task<bool> ExistsBySlugAsync(string slug, string? excludeProductId, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    void Remove(Product product);
}
