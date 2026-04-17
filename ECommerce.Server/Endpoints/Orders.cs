using Application.Common.Models;
using Application.Features.Orders.Commands;
using Application.Features.Orders.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ECommerce.Server.Endpoints;

public sealed class Orders : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetMyOrders)
            .WithSummary("List my orders")
            .WithDescription("Returns all non-draft orders for the current authenticated user.")
            .Produces<IReadOnlyList<OrderDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        groupBuilder.MapGet(GetMyOrderById, "{id}")
            .WithSummary("Get my order")
            .WithDescription("Returns a specific non-draft order owned by the current authenticated user.")
            .Produces<OrderDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        groupBuilder.MapPost(CancelMyOrder, "{id}/cancel")
            .WithSummary("Cancel my order")
            .WithDescription("Cancels an order owned by the current authenticated user when it is still cancellable.")
            .Produces<OrderDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<IReadOnlyList<OrderDto>>> GetMyOrders(ISender sender, CancellationToken cancellationToken)
    {
        var orders = await sender.Send(new GetMyOrdersQuery(), cancellationToken);
        return TypedResults.Ok(orders);
    }

    private static async Task<Ok<OrderDto>> GetMyOrderById(
        string id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var order = await sender.Send(new GetMyOrderByIdQuery(id), cancellationToken);
        return TypedResults.Ok(order);
    }

    private static async Task<Ok<OrderDto>> CancelMyOrder(
        string id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var order = await sender.Send(new CancelMyOrderCommand(id), cancellationToken);
        return TypedResults.Ok(order);
    }
}
