using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

internal sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.StoreId)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ProductVariantId)
            .HasMaxLength(32);

        builder.Property(x => x.QuantityOnHand)
            .HasDefaultValue(0);

        builder.Property(x => x.ReservedQuantity)
            .HasDefaultValue(0);

        builder.Property(x => x.ReorderThreshold)
            .HasDefaultValue(0);

        builder.Ignore(x => x.AvailableQuantity);

        builder.HasOne(x => x.Store)
            .WithMany(x => x.InventoryItems)
            .HasForeignKey(x => x.StoreId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.InventoryItems)
            .HasForeignKey(x => x.ProductId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProductVariant)
            .WithMany(x => x.InventoryItems)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.StoreId, x.ProductId, x.ProductVariantId })
            .IsUnique();

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_InventoryItems_QuantityOnHand_NonNegative", "\"QuantityOnHand\" >= 0");
            table.HasCheckConstraint("CK_InventoryItems_ReservedQuantity_NonNegative", "\"ReservedQuantity\" >= 0");
            table.HasCheckConstraint("CK_InventoryItems_ReorderThreshold_NonNegative", "\"ReorderThreshold\" >= 0");
            table.HasCheckConstraint("CK_InventoryItems_ReservedQuantity_Lte_QuantityOnHand", "\"ReservedQuantity\" <= \"QuantityOnHand\"");
        });
    }
}
