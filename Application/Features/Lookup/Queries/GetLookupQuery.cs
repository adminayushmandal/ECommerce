using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Lookup.Queries;

public sealed record GetLookupQuery() : IRequest<LookupDto>;

public sealed class GetLookupQueryHandler(IUser user, IIdentityService identityService)
    : IRequestHandler<GetLookupQuery, LookupDto>
{
    public async Task<LookupDto> Handle(GetLookupQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            return new LookupDto
            {
                CurrentUser = null
            };
        }

        var currentUser = await identityService.GetUserAsync(user.Id, cancellationToken);

        return new LookupDto
        {
            CurrentUser = currentUser
        };
    }
}
