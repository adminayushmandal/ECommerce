using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.Orders.Queries;

public sealed record GetMyOrderByIdQuery(string OrderId) : IRequest<OrderDto>;

public sealed class GetMyOrderByIdQueryHandler(IOrderRepository orderRepository, IUser user, IMapper mapper)
    : IRequestHandler<GetMyOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetMyOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var order = await orderRepository.GetByIdAsync(userId, request.OrderId, cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);

        if (order.Status == Domain.Enums.OrderStatus.Draft)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        return mapper.Map<OrderDto>(order);
    }
}
