using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.Cart.Commands;

public sealed record CheckoutCartCommand(
    string StoreId,
    string CustomerEmail,
    double CustomerLatitude,
    double CustomerLongitude) : IRequest<OrderDto>;

public sealed class CheckoutCartCommandHandler(
    IOrderRepository orderRepository,
    IStoreRepository storeRepository,
    IApplicationDbContext applicationDbContext,
    IUser user,
    IMapper mapper)
    : IRequestHandler<CheckoutCartCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var cart = await orderRepository.GetDraftByUserIdAsync(userId, cancellationToken)
            ?? throw new CartNotFoundException();

        if (!await storeRepository.ExistsAsync(request.StoreId, cancellationToken))
        {
            throw new InvalidOrderOperationException($"Store '{request.StoreId}' was not found.");
        }

        cart.SetCustomerEmail(request.CustomerEmail);
        cart.SetCustomerLocation(request.CustomerLatitude, request.CustomerLongitude);

        try
        {
            cart.Checkout(request.StoreId);
        }
        catch (InvalidOperationException)
        {
            throw new EmptyCartCheckoutException();
        }

        await applicationDbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<OrderDto>(cart);
    }
}
