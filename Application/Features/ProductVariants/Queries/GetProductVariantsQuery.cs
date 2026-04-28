using Application.Common.Exceptions;
using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductVariants.Queries;

public sealed record GetProductVariantsQuery(string ProductId) : IRequest<IReadOnlyList<ProductVariantDto>>;

public sealed class GetProductVariantsQueryHandler(
    IApplicationDbContext applicationDbContext,
    IApplicationCache applicationCache,
    IMapper mapper)
    : IRequestHandler<GetProductVariantsQuery, IReadOnlyList<ProductVariantDto>>
{
    public async Task<IReadOnlyList<ProductVariantDto>> Handle(GetProductVariantsQuery request, CancellationToken cancellationToken)
    {
        return await applicationCache.GetOrCreateAsync(
            CacheRegions.Catalog,
            $"product:{request.ProductId}:variants",
            CacheDurations.ProductDetails,
            async token =>
            {
                if (!await applicationDbContext.Products.AnyAsync(
                    product => product.Id == request.ProductId && product.IsActive && product.Category.IsActive,
                    token))
                {
                    throw new ProductNotFoundException(request.ProductId);
                }

                return (await applicationDbContext.ProductVariants
                    .AsNoTracking()
                    .Where(productVariant =>
                        productVariant.ProductId == request.ProductId &&
                        productVariant.IsActive)
                    .OrderBy(productVariant => productVariant.Name)
                    .ProjectToListAsync<ProductVariantDto>(mapper.ConfigurationProvider, token)).ToArray();
            },
            cancellationToken);
    }
}
