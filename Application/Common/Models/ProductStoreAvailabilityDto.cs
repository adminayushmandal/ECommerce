namespace Application.Common.Models;

public sealed record ProductStoreAvailabilityDto(
    string StoreId,
    string StoreCode,
    string StoreName,
    string City,
    string State,
    string Country,
    string PostalCode,
    double Latitude,
    double Longitude,
    int AvailableQuantity,
    bool CanFulfill,
    double? DistanceKilometers);
