using Application.Common.Exceptions;
using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Services;
using AutoMapper;
using MediatR;

namespace Application.Features.Cart.Commands;

public sealed record CheckoutCartCommand(
    string? StoreId,
    string CustomerEmail,
    double CustomerLatitude,
    double CustomerLongitude) : IRequest<OrderDto>;

public sealed class CheckoutCartCommandHandler(
    IOrderRepository orderRepository,
    OrderCheckoutService checkoutService,
    IApplicationDbContext applicationDbContext,
    IApplicationCache applicationCache,
    IUser user,
    IMapper mapper)
    : IRequestHandler<CheckoutCartCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var cart = await orderRepository.GetDraftByUserIdAsync(userId, cancellationToken)
            ?? throw new CartNotFoundException();

        cart.SetCustomerEmail(request.CustomerEmail);
        cart.SetCustomerLocation(request.CustomerLatitude, request.CustomerLongitude);

        var allocatedStore = await checkoutService.ResolveAndReserveInventoryAsync(
            cart,
            request.StoreId,
            request.CustomerLatitude,
            request.CustomerLongitude,
            cancellationToken);

        try
        {
            cart.Checkout(allocatedStore.Id);
        }
        catch (InvalidOperationException)
        {
            throw new EmptyCartCheckoutException();
        }

        await applicationDbContext.SaveChangesAsync(cancellationToken);
        await applicationCache.InvalidateRegionAsync(CacheRegions.Catalog, cancellationToken);
        return mapper.Map<OrderDto>(cart);
    }
}
