using Application.Common.Exceptions;
using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Products.Commands;

public sealed record CreateProductCommand(
    string CategoryId,
    string Sku,
    string Name,
    string Slug,
    string Description,
    decimal BasePrice,
    string ImageUrl,
    bool IsActive) : IRequest<ProductDto>;

public sealed class CreateProductCommandHandler(
    ICategoryRepository categoryRepository,
    IProductRepository productRepository,
    IApplicationDbContext applicationDbContext,
    IProductVectorIndexingService productVectorIndexingService,
    IApplicationCache applicationCache,
    IMapper mapper)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (!await categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
        {
            throw new CategoryNotFoundException(request.CategoryId);
        }

        if (await productRepository.ExistsBySkuAsync(request.Sku, null, cancellationToken))
        {
            throw new DuplicateProductSkuException(request.Sku);
        }

        if (await productRepository.ExistsBySlugAsync(request.Slug, null, cancellationToken))
        {
            throw new DuplicateProductSlugException(request.Slug);
        }

        var product = new Product(
            request.CategoryId,
            request.Sku,
            request.Name,
            request.Slug,
            request.Description,
            request.BasePrice,
            request.ImageUrl);

        product.SetActive(request.IsActive);

        await productRepository.AddAsync(product, cancellationToken);
        await applicationDbContext.SaveChangesAsync(cancellationToken);
        await applicationCache.InvalidateRegionAsync(CacheRegions.Catalog, cancellationToken);
        await productVectorIndexingService.IndexProductGraphAsync(product.Id, cancellationToken);

        var createdProduct = await productRepository.GetByIdAsync(product.Id, cancellationToken)
            ?? throw new ProductNotFoundException(product.Id);

        return mapper.Map<ProductDto>(createdProduct);
    }
}
