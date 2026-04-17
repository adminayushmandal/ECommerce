using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.Cart.Commands;

public sealed record RemoveCartItemCommand(string OrderItemId) : IRequest<CartDto>;

public sealed class RemoveCartItemCommandHandler(
    IOrderRepository orderRepository,
    IApplicationDbContext applicationDbContext,
    IUser user,
    IMapper mapper)
    : IRequestHandler<RemoveCartItemCommand, CartDto>
{
    public async Task<CartDto> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var cart = await orderRepository.GetDraftByUserIdAsync(userId, cancellationToken)
            ?? throw new CartNotFoundException();

        try
        {
            cart.RemoveItem(request.OrderItemId);
        }
        catch (InvalidOperationException)
        {
            throw new OrderItemNotFoundException(request.OrderItemId);
        }

        await applicationDbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<CartDto>(cart);
    }
}
