namespace Domain.Entities.Vector
{
    public class ProductVectorRecord
    {
        public string Id { get; set; } = default!;

        public string ProductId { get; set; } = default!;

        public string? ProductVariantId { get; set; }

        public string RecordType { get; set; } = default!;

        public string Sku { get; set; } = default!;

        public string Name { get; set; } = default!;

        public string CategoryName { get; set; } = default!;

        public string ImageUrl { get; set; } = default!;

        public decimal Price { get; set; }

        public bool IsActive { get; set; }

        public string Content { get; set; } = default!;

        public string[] Tags { get; set; } = [];

        public Pgvector.Vector Embedding { get; set; } = null!;
    }
}
