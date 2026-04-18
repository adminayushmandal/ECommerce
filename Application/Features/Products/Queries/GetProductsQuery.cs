using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries;

public sealed record GetProductsQuery(string? StoreId = null) : IRequest<IReadOnlyList<ProductDto>>;

public sealed class GetProductsQueryHandler(IApplicationDbContext applicationDbContext, IStoreRepository storeRepository, IMapper mapper)
    : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.StoreId))
        {
            return await GetStoreCatalogAsync(request.StoreId, cancellationToken);
        }

        return await applicationDbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Name)
            .ProjectToListAsync<ProductDto>(mapper.ConfigurationProvider, cancellationToken);
    }

    private async Task<IReadOnlyList<ProductDto>> GetStoreCatalogAsync(string storeId, CancellationToken cancellationToken)
    {
        var store = await storeRepository.GetByIdAsync(storeId, cancellationToken)
            ?? throw new StoreNotFoundException(storeId);

        var productAvailability = await applicationDbContext.InventoryItems
            .AsNoTracking()
            .Where(inventoryItem =>
                inventoryItem.StoreId == storeId &&
                inventoryItem.QuantityOnHand > inventoryItem.ReservedQuantity)
            .GroupBy(inventoryItem => inventoryItem.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                AvailableQuantity = group.Sum(item => item.QuantityOnHand - item.ReservedQuantity)
            })
            .ToDictionaryAsync(item => item.ProductId, item => item.AvailableQuantity, cancellationToken);

        if (productAvailability.Count == 0)
        {
            return [];
        }

        var productIds = productAvailability.Keys.ToArray();

        var products = await applicationDbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Variants)
            .Where(product => productIds.Contains(product.Id))
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return products
            .Select(product =>
            {
                var dto = mapper.Map<ProductDto>(product);
                return dto with
                {
                    AvailableStoreId = store.Id,
                    AvailableStoreName = store.Name,
                    AvailableQuantity = productAvailability[product.Id]
                };
            })
            .ToArray();
    }
}
