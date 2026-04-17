namespace Application.Common.Models;

public sealed record ProductVariantDto(
    string Id,
    string ProductId,
    string Sku,
    string Name,
    string? AttributeSummary,
    string ImageUrl,
    decimal? PriceOverride,
    decimal EffectivePrice,
    bool IsActive);
