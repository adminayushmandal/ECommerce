namespace Infrastructure.Data.SeedData;

internal sealed record CategorySeed(string Slug, string Name, string Description, string Color);

internal sealed record ProductSeed(
    string CategorySlug,
    string Sku,
    string Slug,
    string Name,
    string Description,
    decimal BasePrice,
    string Color,
    IReadOnlyList<ProductVariantSeed> Variants);

internal sealed record ProductVariantSeed(
    string Sku,
    string Name,
    string AttributeSummary,
    string Color,
    decimal? PriceOverride);

internal sealed record StoreSeed(
    string Code,
    string Name,
    string AddressLine1,
    string City,
    string State,
    string Country,
    string PostalCode,
    double Latitude,
    double Longitude,
    string? AddressLine2 = null);

internal sealed record InventorySeedKey(string ProductId, string? ProductVariantId);
