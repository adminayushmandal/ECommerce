namespace Domain.Entities;

public sealed class Category : BaseAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public ICollection<Product> Products { get; private set; } = [];

    private Category()
    {
    }

    public Category(string name, string slug, string description, string imageUrl)
    {
        Name = name;
        Slug = slug;
        Description = description;
        ImageUrl = imageUrl;
    }

    public void UpdateDetails(string name, string slug, string description, string imageUrl)
    {
        Name = name;
        Slug = slug;
        Description = description;
        ImageUrl = imageUrl;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
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
    public string ImageUrl { get; private set; } = string.Empty;
    public decimal BasePrice { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<ProductVariant> Variants { get; private set; } = [];
    public ICollection<InventoryItem> InventoryItems { get; private set; } = [];

    private Product()
    {
    }

    public Product(string categoryId, string sku, string name, string slug, string description, decimal basePrice, string imageUrl)
    {
        CategoryId = categoryId;
        Sku = sku;
        Name = name;
        Slug = slug;
        Description = description;
        BasePrice = basePrice;
        ImageUrl = imageUrl;
    }

    public void UpdateDetails(string categoryId, string sku, string name, string slug, string description, decimal basePrice, string imageUrl)
    {
        CategoryId = categoryId;
        Sku = sku;
        Name = name;
        Slug = slug;
        Description = description;
        BasePrice = basePrice;
        ImageUrl = imageUrl;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}

public sealed class ProductVariant : BaseAuditableEntity
{
    public string ProductId { get; private set; } = string.Empty;
    public Product Product { get; private set; } = default!;
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? AttributeSummary { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public decimal? PriceOverride { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<InventoryItem> InventoryItems { get; private set; } = [];
    public decimal EffectivePrice => PriceOverride ?? Product?.BasePrice ?? 0m;

    private ProductVariant()
    {
    }

    public ProductVariant(string productId, string sku, string name, string? attributeSummary, string imageUrl, decimal? priceOverride = null)
    {
        ProductId = productId;
        Sku = sku;
        Name = name;
        AttributeSummary = attributeSummary;
        ImageUrl = imageUrl;
        PriceOverride = priceOverride;
    }

    public void UpdateDetails(string sku, string name, string? attributeSummary, string imageUrl, decimal? priceOverride)
    {
        Sku = sku;
        Name = name;
        AttributeSummary = attributeSummary;
        ImageUrl = imageUrl;
        PriceOverride = priceOverride;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
