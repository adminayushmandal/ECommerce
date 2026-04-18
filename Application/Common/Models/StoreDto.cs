namespace Application.Common.Models;

public sealed record StoreDto(
    string Id,
    string Code,
    string Name,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string Country,
    string PostalCode,
    double Latitude,
    double Longitude,
    bool IsActive);
