using Application.Common.Exceptions;
using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Application.Features.Stores.Queries;

public sealed record GetProductStoreAvailabilityQuery(
    string ProductId,
    string? ProductVariantId,
    double? CustomerLatitude,
    double? CustomerLongitude) : IRequest<IReadOnlyList<ProductStoreAvailabilityDto>>;

public sealed class GetProductStoreAvailabilityQueryHandler(
    IApplicationDbContext applicationDbContext,
    IProductRepository productRepository,
    IProductVariantRepository productVariantRepository,
    IApplicationCache applicationCache)
    : IRequestHandler<GetProductStoreAvailabilityQuery, IReadOnlyList<ProductStoreAvailabilityDto>>
{
    public async Task<IReadOnlyList<ProductStoreAvailabilityDto>> Handle(GetProductStoreAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var roundedLatitude = request.CustomerLatitude.HasValue
            ? Math.Round(request.CustomerLatitude.Value, 4, MidpointRounding.AwayFromZero).ToString("F4", CultureInfo.InvariantCulture)
            : "none";
        var roundedLongitude = request.CustomerLongitude.HasValue
            ? Math.Round(request.CustomerLongitude.Value, 4, MidpointRounding.AwayFromZero).ToString("F4", CultureInfo.InvariantCulture)
            : "none";

        return await applicationCache.GetOrCreateAsync(
            CacheRegions.Catalog,
            $"availability:product:{request.ProductId}:variant:{request.ProductVariantId ?? "none"}:lat:{roundedLatitude}:lon:{roundedLongitude}",
            CacheDurations.ProductAvailability,
            token => GetAvailabilityAsync(request, token),
            cancellationToken);
    }

    private async Task<ProductStoreAvailabilityDto[]> GetAvailabilityAsync(
        GetProductStoreAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        if (!product.IsActive || !product.Category.IsActive)
        {
            throw new ProductNotFoundException(request.ProductId);
        }

        if (!string.IsNullOrWhiteSpace(request.ProductVariantId))
        {
            _ = await productVariantRepository.GetByIdAsync(request.ProductId, request.ProductVariantId!, cancellationToken)
                ?? throw new ProductVariantNotFoundException(request.ProductVariantId);
        }

        var inventoryQuery = applicationDbContext.InventoryItems
            .AsNoTracking()
            .Include(inventoryItem => inventoryItem.Store)
            .Where(inventoryItem =>
                inventoryItem.ProductId == request.ProductId &&
                inventoryItem.Product.IsActive &&
                inventoryItem.Product.Category.IsActive &&
                (inventoryItem.ProductVariantId == null || inventoryItem.ProductVariant!.IsActive) &&
                inventoryItem.Store.IsActive);

        if (!string.IsNullOrWhiteSpace(request.ProductVariantId))
        {
            inventoryQuery = inventoryQuery.Where(inventoryItem => inventoryItem.ProductVariantId == request.ProductVariantId);
        }

        var inventoryItems = await inventoryQuery.ToListAsync(cancellationToken);

        return inventoryItems
            .GroupBy(inventoryItem => inventoryItem.Store)
            .Select(group =>
            {
                var store = group.Key;
                var availableQuantity = group.Sum(item => item.AvailableQuantity);
                var distanceKilometers = request.CustomerLatitude.HasValue && request.CustomerLongitude.HasValue
                    ? (double?)CalculateDistanceKilometers(
                        request.CustomerLatitude.Value,
                        request.CustomerLongitude.Value,
                        store.Latitude,
                        store.Longitude)
                    : null;

                return new ProductStoreAvailabilityDto(
                    store.Id,
                    store.Code,
                    store.Name,
                    store.City,
                    store.State,
                    store.Country,
                    store.PostalCode,
                    store.Latitude,
                    store.Longitude,
                    availableQuantity,
                    availableQuantity > 0,
                    distanceKilometers);
            })
            .OrderBy(result => result.DistanceKilometers ?? double.MaxValue)
            .ThenBy(result => result.StoreName)
            .ToArray();
    }

    private static double CalculateDistanceKilometers(
        double originLatitude,
        double originLongitude,
        double destinationLatitude,
        double destinationLongitude)
    {
        const double earthRadiusKilometers = 6371d;

        static double ToRadians(double degrees) => degrees * (Math.PI / 180d);

        var latitudeDifference = ToRadians(destinationLatitude - originLatitude);
        var longitudeDifference = ToRadians(destinationLongitude - originLongitude);

        var originLatitudeRadians = ToRadians(originLatitude);
        var destinationLatitudeRadians = ToRadians(destinationLatitude);

        var a =
            Math.Sin(latitudeDifference / 2d) * Math.Sin(latitudeDifference / 2d) +
            Math.Cos(originLatitudeRadians) * Math.Cos(destinationLatitudeRadians) *
            Math.Sin(longitudeDifference / 2d) * Math.Sin(longitudeDifference / 2d);

        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        return Math.Round(earthRadiusKilometers * c, 2, MidpointRounding.AwayFromZero);
    }
}
