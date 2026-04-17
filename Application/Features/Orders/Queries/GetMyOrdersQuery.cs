using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.Orders.Queries;

public sealed record GetMyOrdersQuery() : IRequest<IReadOnlyList<OrderDto>>;

public sealed class GetMyOrdersQueryHandler(IOrderRepository orderRepository, IUser user, IMapper mapper)
    : IRequestHandler<GetMyOrdersQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var orders = await orderRepository.GetSubmittedByUserIdAsync(userId, cancellationToken);

        return mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }
}
