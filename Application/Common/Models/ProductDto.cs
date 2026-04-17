namespace Application.Common.Models;

public sealed record ProductDto(
    string Id,
    string CategoryId,
    string CategoryName,
    string Sku,
    string Name,
    string Slug,
    string Description,
    string ImageUrl,
    decimal BasePrice,
    bool IsActive,
    IReadOnlyList<ProductVariantDto> Variants);
