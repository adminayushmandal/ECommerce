using Domain.Entities.Vector;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    internal sealed class ProductVectorRecordConfiguration : IEntityTypeConfiguration<ProductVectorRecord>
    {
        public void Configure(EntityTypeBuilder<ProductVectorRecord> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Embedding)
                .HasColumnType("vector(768)");
        }
    }
}
