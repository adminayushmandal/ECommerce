using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

internal sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.ProductId)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AttributeSummary)
            .HasMaxLength(500);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(x => x.PriceOverride)
            .HasPrecision(18, 2);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Ignore(x => x.EffectivePrice);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Variants)
            .HasForeignKey(x => x.ProductId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Sku)
            .IsUnique();

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_ProductVariants_PriceOverride_NonNegative", "\"PriceOverride\" IS NULL OR \"PriceOverride\" >= 0");
        });
    }
}
