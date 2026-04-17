using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class ProductVariantRepository(ApplicationDbContext dbContext) : IProductVariantRepository
{
    public async Task<IReadOnlyList<ProductVariant>> GetByProductIdAsync(string productId, CancellationToken cancellationToken)
    {
        return await dbContext.ProductVariants
            .AsNoTracking()
            .Include(productVariant => productVariant.Product)
            .Where(productVariant => productVariant.ProductId == productId)
            .OrderBy(productVariant => productVariant.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<ProductVariant?> GetByIdAsync(string productVariantId, CancellationToken cancellationToken)
    {
        return dbContext.ProductVariants
            .Include(productVariant => productVariant.Product)
            .FirstOrDefaultAsync(productVariant => productVariant.Id == productVariantId, cancellationToken);
    }

    public Task<ProductVariant?> GetByIdAsync(string productId, string productVariantId, CancellationToken cancellationToken)
    {
        return dbContext.ProductVariants
            .Include(productVariant => productVariant.Product)
            .FirstOrDefaultAsync(
                productVariant => productVariant.ProductId == productId && productVariant.Id == productVariantId,
                cancellationToken);
    }

    public Task<bool> ExistsBySkuAsync(string sku, string? excludeProductVariantId, CancellationToken cancellationToken)
    {
        return dbContext.ProductVariants.AnyAsync(
            productVariant => productVariant.Sku == sku && (excludeProductVariantId == null || productVariant.Id != excludeProductVariantId),
            cancellationToken);
    }

    public Task AddAsync(ProductVariant productVariant, CancellationToken cancellationToken)
    {
        return dbContext.ProductVariants.AddAsync(productVariant, cancellationToken).AsTask();
    }

    public void Remove(ProductVariant productVariant)
    {
        dbContext.ProductVariants.Remove(productVariant);
    }
}
