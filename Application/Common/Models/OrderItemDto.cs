namespace Application.Common.Models;

public sealed record OrderItemDto(
    string Id,
    string ProductId,
    string? ProductVariantId,
    string ProductName,
    string? VariantName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
