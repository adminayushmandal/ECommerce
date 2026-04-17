using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IProductVariantRepository
{
    Task<IReadOnlyList<ProductVariant>> GetByProductIdAsync(string productId, CancellationToken cancellationToken);
    Task<ProductVariant?> GetByIdAsync(string productVariantId, CancellationToken cancellationToken);
    Task<ProductVariant?> GetByIdAsync(string productId, string productVariantId, CancellationToken cancellationToken);
    Task<bool> ExistsBySkuAsync(string sku, string? excludeProductVariantId, CancellationToken cancellationToken);
    Task AddAsync(ProductVariant productVariant, CancellationToken cancellationToken);
    void Remove(ProductVariant productVariant);
}
