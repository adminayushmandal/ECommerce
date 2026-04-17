using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.Cart.Queries;

public sealed record GetMyCartQuery() : IRequest<CartDto>;

public sealed class GetMyCartQueryHandler(IOrderRepository orderRepository, IUser user, IMapper mapper)
    : IRequestHandler<GetMyCartQuery, CartDto>
{
    public async Task<CartDto> Handle(GetMyCartQuery request, CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new CurrentUserUnavailableException();
        var cart = await orderRepository.GetDraftByUserIdAsync(userId, cancellationToken);

        return cart is null
            ? new CartDto(null, null, userId, string.Empty, 0, 0, 0, [], true)
            : mapper.Map<CartDto>(cart);
    }
}
