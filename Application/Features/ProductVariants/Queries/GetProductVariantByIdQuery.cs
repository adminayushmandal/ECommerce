using Application.Common.Exceptions;
using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.ProductVariants.Queries;

public sealed record GetProductVariantByIdQuery(string ProductId, string ProductVariantId) : IRequest<ProductVariantDto>;

public sealed class GetProductVariantByIdQueryHandler(
    IProductVariantRepository productVariantRepository,
    IApplicationCache applicationCache,
    IMapper mapper)
    : IRequestHandler<GetProductVariantByIdQuery, ProductVariantDto>
{
    public async Task<ProductVariantDto> Handle(GetProductVariantByIdQuery request, CancellationToken cancellationToken)
    {
        return await applicationCache.GetOrCreateAsync(
            CacheRegions.Catalog,
            $"product:{request.ProductId}:variant:{request.ProductVariantId}",
            CacheDurations.ProductDetails,
            async token =>
            {
                var productVariant = await productVariantRepository.GetByIdAsync(request.ProductId, request.ProductVariantId, token)
                    ?? throw new ProductVariantNotFoundException(request.ProductVariantId);

                if (!productVariant.IsActive || !productVariant.Product.IsActive || !productVariant.Product.Category.IsActive)
                {
                    throw new ProductVariantNotFoundException(request.ProductVariantId);
                }

                return mapper.Map<ProductVariantDto>(productVariant);
            },
            cancellationToken);
    }
}
