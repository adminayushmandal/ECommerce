using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class CategoryRepository(ApplicationDbContext dbContext) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Category?> GetByIdAsync(string categoryId, CancellationToken cancellationToken)
    {
        return dbContext.Categories
            .FirstOrDefaultAsync(category => category.Id == categoryId, cancellationToken);
    }

    public Task<bool> ExistsAsync(string categoryId, CancellationToken cancellationToken)
    {
        return dbContext.Categories.AnyAsync(category => category.Id == categoryId, cancellationToken);
    }
}
