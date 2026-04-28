using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.OrderId)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ProviderOrderId)
            .HasMaxLength(128);

        builder.Property(x => x.ProviderCaptureId)
            .HasMaxLength(128);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.FailureReason)
            .HasMaxLength(1000);

        builder.Property(x => x.RefundedAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.OrderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => new { x.Provider, x.ProviderOrderId })
            .IsUnique();

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Payments_Amount_Positive", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_Payments_RefundedAmount_NonNegative", "\"RefundedAmount\" >= 0");
            table.HasCheckConstraint("CK_Payments_RefundedAmount_MaxAmount", "\"RefundedAmount\" <= \"Amount\"");
        });
    }
}
