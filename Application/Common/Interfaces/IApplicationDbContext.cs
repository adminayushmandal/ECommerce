using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Category> Categories { get; }

        DbSet<Product> Products { get; }

        DbSet<ProductVariant> ProductVariants { get; }

        DbSet<Store> Stores { get; }

        DbSet<InventoryItem> InventoryItems { get; }

        DbSet<Order> Orders { get; }

        DbSet<OrderItem> OrderItems { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
