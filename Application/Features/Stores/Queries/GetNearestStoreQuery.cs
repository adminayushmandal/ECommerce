using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Stores.Queries;

public sealed record GetNearestStoreQuery(double CustomerLatitude, double CustomerLongitude) : IRequest<NearestStoreDto>;

public sealed class GetNearestStoreQueryHandler(IStoreRepository storeRepository)
    : IRequestHandler<GetNearestStoreQuery, NearestStoreDto>
{
    public async Task<NearestStoreDto> Handle(GetNearestStoreQuery request, CancellationToken cancellationToken)
    {
        var store = (await storeRepository.GetAllAsync(cancellationToken))
            .Where(candidate => candidate.IsActive)
            .OrderBy(candidate => CalculateDistanceKilometers(
                request.CustomerLatitude,
                request.CustomerLongitude,
                candidate.Latitude,
                candidate.Longitude))
            .ThenBy(candidate => candidate.Name)
            .FirstOrDefault();

        if (store is null)
        {
            throw new UnableToAllocateStoreException("No active store is available for the supplied location.");
        }

        return new NearestStoreDto(
            store.Id,
            store.Code,
            store.Name,
            store.AddressLine1,
            store.AddressLine2,
            store.City,
            store.State,
            store.Country,
            store.PostalCode,
            store.Latitude,
            store.Longitude,
            store.IsActive,
            Math.Round(
                CalculateDistanceKilometers(
                    request.CustomerLatitude,
                    request.CustomerLongitude,
                    store.Latitude,
                    store.Longitude),
                2,
                MidpointRounding.AwayFromZero));
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
        return earthRadiusKilometers * c;
    }
}
