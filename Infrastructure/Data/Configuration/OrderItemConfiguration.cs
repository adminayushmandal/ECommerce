using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.OrderId)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ProductVariantId)
            .HasMaxLength(32);

        builder.Property(x => x.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.VariantName)
            .HasMaxLength(100);

        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        builder.Ignore(x => x.LineTotal);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.OrderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "\"Quantity\" > 0");
            table.HasCheckConstraint("CK_OrderItems_UnitPrice_NonNegative", "\"UnitPrice\" >= 0");
        });
    }
}
