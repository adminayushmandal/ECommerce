using Application.Common.Exceptions;
using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.Products.Queries;

public sealed record GetProductByIdQuery(string ProductId) : IRequest<ProductDto>;

public sealed class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    IApplicationCache applicationCache,
    IMapper mapper)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return await applicationCache.GetOrCreateAsync(
            CacheRegions.Catalog,
            $"product:{request.ProductId}",
            CacheDurations.ProductDetails,
            async token =>
            {
                var product = await productRepository.GetByIdAsync(request.ProductId, token)
                    ?? throw new ProductNotFoundException(request.ProductId);

                if (!product.IsActive || !product.Category.IsActive)
                {
                    throw new ProductNotFoundException(request.ProductId);
                }

                return mapper.Map<ProductDto>(product);
            },
            cancellationToken);
    }
}
