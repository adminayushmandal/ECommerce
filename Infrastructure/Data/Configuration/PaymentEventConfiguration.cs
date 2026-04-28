using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

internal sealed class PaymentEventConfiguration : IEntityTypeConfiguration<PaymentEvent>
{
    public void Configure(EntityTypeBuilder<PaymentEvent> builder)
    {
        builder.ToTable("PaymentEvents");
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.PaymentId)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ProviderEventId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.RawPayload)
            .HasColumnType("text");

        builder.HasOne(x => x.Payment)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.PaymentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.ProviderEventId)
            .IsUnique();
    }
}
