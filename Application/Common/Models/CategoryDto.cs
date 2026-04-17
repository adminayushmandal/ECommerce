namespace Application.Common.Models;

public sealed record CategoryDto(
    string Id,
    string Name,
    string Slug,
    string Description,
    string ImageUrl,
    bool IsActive);
