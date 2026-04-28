using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.Stores.Queries;

public sealed record GetStoresQuery() : IRequest<IReadOnlyList<StoreDto>>;

public sealed class GetStoresQueryHandler(
    IStoreRepository storeRepository,
    IApplicationCache applicationCache,
    IMapper mapper)
    : IRequestHandler<GetStoresQuery, IReadOnlyList<StoreDto>>
{
    public async Task<IReadOnlyList<StoreDto>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
    {
        return await applicationCache.GetOrCreateAsync(
            CacheRegions.Stores,
            "stores:active",
            CacheDurations.Stores,
            async token =>
            {
                var stores = await storeRepository.GetAllAsync(token);
                return stores
                    .Where(store => store.IsActive)
                    .Select(store => mapper.Map<StoreDto>(store))
                    .OrderBy(store => store.Name)
                    .ToArray();
            },
            cancellationToken);
    }
}
