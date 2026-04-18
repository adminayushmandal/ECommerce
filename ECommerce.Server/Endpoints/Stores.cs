using Application.Common.Models;
using Application.Features.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ECommerce.Server.Endpoints;

public sealed class Stores : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetStores)
            .WithSummary("List stores")
            .WithDescription("Returns all active stores available for fulfillment and customer-facing discovery.")
            .Produces<IReadOnlyList<StoreDto>>();

        groupBuilder.MapGet(GetNearestStore, "nearest")
            .WithSummary("Get nearest store")
            .WithDescription("Returns the nearest active store for the supplied customer coordinates.")
            .Produces<NearestStoreDto>()
            .ProducesProblem(StatusCodes.Status409Conflict);

        groupBuilder.MapGet(GetProductAvailability, "availability/products/{productId}")
            .WithSummary("Get product availability by store")
            .WithDescription("Returns store availability for a product, optionally scoped to a product variant and ordered by customer distance when coordinates are provided.")
            .Produces<IReadOnlyList<ProductStoreAvailabilityDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<IReadOnlyList<StoreDto>>> GetStores(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var stores = await sender.Send(new GetStoresQuery(), cancellationToken);
        return TypedResults.Ok(stores);
    }

    private static async Task<Ok<NearestStoreDto>> GetNearestStore(
        double customerLatitude,
        double customerLongitude,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var store = await sender.Send(new GetNearestStoreQuery(customerLatitude, customerLongitude), cancellationToken);
        return TypedResults.Ok(store);
    }

    private static async Task<Ok<IReadOnlyList<ProductStoreAvailabilityDto>>> GetProductAvailability(
        string productId,
        string? productVariantId,
        double? customerLatitude,
        double? customerLongitude,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var availability = await sender.Send(
            new GetProductStoreAvailabilityQuery(productId, productVariantId, customerLatitude, customerLongitude),
            cancellationToken);

        return TypedResults.Ok(availability);
    }
}
