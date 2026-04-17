using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Variants)
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(string productId, CancellationToken cancellationToken)
    {
        return dbContext.Products
            .Include(product => product.Category)
            .Include(product => product.Variants)
            .FirstOrDefaultAsync(product => product.Id == productId, cancellationToken);
    }

    public Task<bool> ExistsBySkuAsync(string sku, string? excludeProductId, CancellationToken cancellationToken)
    {
        return dbContext.Products.AnyAsync(
            product => product.Sku == sku && (excludeProductId == null || product.Id != excludeProductId),
            cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(string slug, string? excludeProductId, CancellationToken cancellationToken)
    {
        return dbContext.Products.AnyAsync(
            product => product.Slug == slug && (excludeProductId == null || product.Id != excludeProductId),
            cancellationToken);
    }

    public Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        return dbContext.Products.AddAsync(product, cancellationToken).AsTask();
    }

    public void Remove(Product product)
    {
        dbContext.Products.Remove(product);
    }
}
