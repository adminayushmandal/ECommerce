using Application.Common.Models;
using Application.Features.Cart.Commands;
using Application.Features.Cart.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ECommerce.Server.Endpoints;

public sealed class Cart : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetCart)
            .WithSummary("Get current cart")
            .WithDescription("Returns the current authenticated user's draft cart.")
            .Produces<CartDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        groupBuilder.MapPost(AddCartItem, "items")
            .WithSummary("Add cart item")
            .WithDescription("Adds a product or product variant to the current authenticated user's cart.")
            .Produces<CartDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        groupBuilder.MapPut(UpdateCartItemQuantity, "items/{orderItemId}")
            .WithSummary("Update cart item quantity")
            .WithDescription("Updates the quantity of an item in the current authenticated user's cart.")
            .Produces<CartDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        groupBuilder.MapDelete(RemoveCartItem, "items/{orderItemId}")
            .WithSummary("Remove cart item")
            .WithDescription("Removes an item from the current authenticated user's cart.")
            .Produces<CartDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        groupBuilder.MapPost(CheckoutCart, "checkout")
            .WithSummary("Checkout cart")
            .WithDescription("Converts the current authenticated user's draft cart into an order and allocates the nearest store with sufficient stock when a store is not explicitly selected.")
            .Produces<OrderDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<CartDto>> GetCart(ISender sender, CancellationToken cancellationToken)
    {
        var cart = await sender.Send(new GetMyCartQuery(), cancellationToken);
        return TypedResults.Ok(cart);
    }

    private static async Task<Ok<CartDto>> AddCartItem(
        AddCartItemRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var cart = await sender.Send(
            new AddCartItemCommand(
                request.ProductId,
                request.ProductVariantId,
                request.Quantity,
                request.CustomerEmail,
                request.CustomerLatitude,
                request.CustomerLongitude),
            cancellationToken);

        return TypedResults.Ok(cart);
    }

    private static async Task<Ok<CartDto>> UpdateCartItemQuantity(
        string orderItemId,
        UpdateCartItemQuantityRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var cart = await sender.Send(new UpdateCartItemQuantityCommand(orderItemId, request.Quantity), cancellationToken);
        return TypedResults.Ok(cart);
    }

    private static async Task<Ok<CartDto>> RemoveCartItem(
        string orderItemId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var cart = await sender.Send(new RemoveCartItemCommand(orderItemId), cancellationToken);
        return TypedResults.Ok(cart);
    }

    private static async Task<Ok<OrderDto>> CheckoutCart(
        CheckoutCartRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var order = await sender.Send(
            new CheckoutCartCommand(
                request.StoreId,
                request.CustomerEmail,
                request.CustomerLatitude,
                request.CustomerLongitude),
            cancellationToken);

        return TypedResults.Ok(order);
    }

    public sealed record AddCartItemRequest(
        string ProductId,
        string? ProductVariantId,
        int Quantity,
        string CustomerEmail,
        double CustomerLatitude,
        double CustomerLongitude);

    public sealed record UpdateCartItemQuantityRequest(int Quantity);

    public sealed record CheckoutCartRequest(
        string? StoreId,
        string CustomerEmail,
        double CustomerLatitude,
        double CustomerLongitude);
}
