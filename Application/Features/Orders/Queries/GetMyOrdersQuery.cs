using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public sealed record GetMyOrdersQuery() : IRequest<IReadOnlyList<OrderDto>>;

public sealed class GetMyOrdersQueryHandler(IApplicationDbContext applicationDbContext, IUser user, IMapper mapper)
    : IRequestHandler<GetMyOrdersQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();

        return await applicationDbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId && order.Status != Domain.Enums.OrderStatus.Draft)
            .OrderByDescending(order => order.CreatedAt)
            .ProjectToListAsync<OrderDto>(mapper.ConfigurationProvider, cancellationToken);
    }
}
