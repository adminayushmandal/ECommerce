using Application.Common.Models;
using Application.Features.Payments.Commands;
using Application.Features.Payments.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ECommerce.Server.Endpoints;

public sealed class Payments : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetPayPalConfig, "config")
            .RequireAuthorization()
            .WithSummary("Get PayPal client config")
            .WithDescription("Returns public PayPal checkout settings needed by the client.")
            .Produces<PayPalClientConfigDto>();

        groupBuilder.MapPost(CreatePayPalOrder, "orders")
            .RequireAuthorization()
            .WithSummary("Create PayPal order")
            .WithDescription("Creates a local pending payment from the current cart and starts a PayPal checkout order.")
            .Produces<PayPalCreateOrderDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        groupBuilder.MapPost(CapturePayPalOrder, "orders/{payPalOrderId}/capture")
            .RequireAuthorization()
            .WithSummary("Capture PayPal order")
            .WithDescription("Captures an approved PayPal order and confirms the local order payment.")
            .Produces<PayPalCaptureDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        groupBuilder.MapPost(CancelPayPalOrder, "orders/{payPalOrderId}/cancel")
            .RequireAuthorization()
            .WithSummary("Cancel PayPal order")
            .WithDescription("Cancels the local pending payment and releases reserved inventory after a PayPal checkout cancellation.")
            .Produces<PayPalCaptureDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<PayPalClientConfigDto>> GetPayPalConfig(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var config = await sender.Send(new GetPayPalClientConfigQuery(), cancellationToken);
        return TypedResults.Ok(config);
    }

    private static async Task<Ok<PayPalCreateOrderDto>> CreatePayPalOrder(
        CreatePayPalOrderRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var order = await sender.Send(
            new CreatePayPalOrderCommand(
                request.StoreId,
                request.CustomerEmail,
                request.CustomerLatitude,
                request.CustomerLongitude,
                request.ReturnUrl,
                request.CancelUrl),
            cancellationToken);

        return TypedResults.Ok(order);
    }

    private static async Task<Ok<PayPalCaptureDto>> CapturePayPalOrder(
        string payPalOrderId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var capture = await sender.Send(new CapturePayPalOrderCommand(payPalOrderId), cancellationToken);
        return TypedResults.Ok(capture);
    }

    private static async Task<Ok<PayPalCaptureDto>> CancelPayPalOrder(
        string payPalOrderId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var cancellation = await sender.Send(new CancelPayPalOrderCommand(payPalOrderId), cancellationToken);
        return TypedResults.Ok(cancellation);
    }

    public sealed record CreatePayPalOrderRequest(
        string? StoreId,
        string CustomerEmail,
        double CustomerLatitude,
        double CustomerLongitude,
        string ReturnUrl,
        string CancelUrl);
}
