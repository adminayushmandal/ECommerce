namespace Domain.Entities;

public sealed class Category : BaseAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public ICollection<Product> Products { get; private set; } = [];

    private Category()
    {
    }

    public Category(string name, string slug, string description)
    {
        Name = name;
        Slug = slug;
        Description = description;
    }
}

public sealed class Product : BaseAuditableEntity
{
    public string CategoryId { get; private set; } = string.Empty;
    public Category Category { get; private set; } = default!;
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal BasePrice { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<ProductVariant> Variants { get; private set; } = [];
    public ICollection<InventoryItem> InventoryItems { get; private set; } = [];

    private Product()
    {
    }

    public Product(string categoryId, string sku, string name, string slug, string description, decimal basePrice)
    {
        CategoryId = categoryId;
        Sku = sku;
        Name = name;
        Slug = slug;
        Description = description;
        BasePrice = basePrice;
    }
}

public sealed class ProductVariant : BaseAuditableEntity
{
    public string ProductId { get; private set; } = string.Empty;
    public Product Product { get; private set; } = default!;
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? AttributeSummary { get; private set; }
    public decimal? PriceOverride { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<InventoryItem> InventoryItems { get; private set; } = [];
    public decimal EffectivePrice => PriceOverride ?? Product?.BasePrice ?? 0m;

    private ProductVariant()
    {
    }

    public ProductVariant(string productId, string sku, string name, string? attributeSummary, decimal? priceOverride = null)
    {
        ProductId = productId;
        Sku = sku;
        Name = name;
        AttributeSummary = attributeSummary;
        PriceOverride = priceOverride;
    }
}
