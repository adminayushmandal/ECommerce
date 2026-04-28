using Application.Common.Models;
using Application.Features.Payments.Queries;
using Application.Features.Products.Queries;
using Application.Features.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Constants;

namespace ECommerce.Server.Endpoints;

public sealed class StoreManager : EndpointGroupBase
{
    public override string? GroupName { get; set; } = "StoreManager";

    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetProducts, "products")
            .RequireAuthorization(Contracts.Products.View)
            .WithSummary("List manager products")
            .WithDescription("Returns catalog products for store manager product operations.")
            .Produces<IReadOnlyList<ProductDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        groupBuilder.MapGet(GetStores, "stores")
            .RequireAuthorization(Contracts.Stores.View)
            .WithSummary("List manager stores")
            .WithDescription("Returns stores visible to store manager operations.")
            .Produces<IReadOnlyList<StoreDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        groupBuilder.MapGet(GetPayments, "payments")
            .RequireAuthorization(Contracts.Payments.View)
            .WithSummary("List manager payments")
            .WithDescription("Returns recent payments for store manager reconciliation.")
            .Produces<IReadOnlyList<StoreManagerPaymentDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<Ok<IReadOnlyList<ProductDto>>> GetProducts(
        string? storeId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var products = await sender.Send(new GetProductsQuery(storeId), cancellationToken);
        return TypedResults.Ok(products);
    }

    private static async Task<Ok<IReadOnlyList<StoreDto>>> GetStores(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var stores = await sender.Send(new GetStoresQuery(), cancellationToken);
        return TypedResults.Ok(stores);
    }

    private static async Task<Ok<IReadOnlyList<StoreManagerPaymentDto>>> GetPayments(
        string? storeId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var payments = await sender.Send(new GetStoreManagerPaymentsQuery(storeId), cancellationToken);
        return TypedResults.Ok(payments);
    }
}
