using Domain.Entities;
using Domain.Entities.Vector;
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

        DbSet<ProductVectorRecord> ProductVectorRecords { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
