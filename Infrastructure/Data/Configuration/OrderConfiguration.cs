using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.OrderNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.CustomerEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.AllocatedStoreId)
            .HasMaxLength(32);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AllocatedStore)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.AllocatedStoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrderNumber)
            .IsUnique();

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Orders_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
        });
    }
}
