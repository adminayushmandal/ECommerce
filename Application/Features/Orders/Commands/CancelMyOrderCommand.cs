using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.Orders.Commands;

public sealed record CancelMyOrderCommand(string OrderId) : IRequest<OrderDto>;

public sealed class CancelMyOrderCommandHandler(
    IOrderRepository orderRepository,
    IApplicationDbContext applicationDbContext,
    IUser user,
    IMapper mapper)
    : IRequestHandler<CancelMyOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CancelMyOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var order = await orderRepository.GetByIdAsync(userId, request.OrderId, cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);

        if (order.Status == Domain.Enums.OrderStatus.Draft)
        {
            throw new InvalidOrderOperationException("Draft carts cannot be cancelled through the orders workflow.");
        }

        try
        {
            order.Cancel();
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOrderOperationException(exception.Message);
        }

        await applicationDbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<OrderDto>(order);
    }
}
