using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.Cart.Commands;

public sealed record UpdateCartItemQuantityCommand(string OrderItemId, int Quantity) : IRequest<CartDto>;

public sealed class UpdateCartItemQuantityCommandHandler(
    IOrderRepository orderRepository,
    IApplicationDbContext applicationDbContext,
    IUser user,
    IMapper mapper)
    : IRequestHandler<UpdateCartItemQuantityCommand, CartDto>
{
    public async Task<CartDto> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var cart = await orderRepository.GetDraftByUserIdAsync(userId, cancellationToken)
            ?? throw new CartNotFoundException();

        try
        {
            cart.UpdateItemQuantity(request.OrderItemId, request.Quantity);
        }
        catch (InvalidOperationException)
        {
            throw new OrderItemNotFoundException(request.OrderItemId);
        }

        await applicationDbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<CartDto>(cart);
    }
}
