using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.Stores.Queries;

public sealed record GetStoresQuery() : IRequest<IReadOnlyList<StoreDto>>;

public sealed class GetStoresQueryHandler(IStoreRepository storeRepository, IMapper mapper)
    : IRequestHandler<GetStoresQuery, IReadOnlyList<StoreDto>>
{
    public async Task<IReadOnlyList<StoreDto>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
    {
        var stores = await storeRepository.GetAllAsync(cancellationToken);
        return stores
            .Where(store => store.IsActive)
            .Select(store => mapper.Map<StoreDto>(store))
            .OrderBy(store => store.Name)
            .ToArray();
    }
}
