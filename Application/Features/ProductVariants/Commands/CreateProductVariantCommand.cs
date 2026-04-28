using Application.Common.Exceptions;
using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.ProductVariants.Commands;

public sealed record CreateProductVariantCommand(
    string ProductId,
    string Sku,
    string Name,
    string? AttributeSummary,
    string ImageUrl,
    decimal? PriceOverride,
    bool IsActive) : IRequest<ProductVariantDto>;

public sealed class CreateProductVariantCommandHandler(
    IProductRepository productRepository,
    IProductVariantRepository productVariantRepository,
    IApplicationDbContext applicationDbContext,
    IProductVectorIndexingService productVectorIndexingService,
    IApplicationCache applicationCache,
    IMapper mapper)
    : IRequestHandler<CreateProductVariantCommand, ProductVariantDto>
{
    public async Task<ProductVariantDto> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {
        if (await productRepository.GetByIdAsync(request.ProductId, cancellationToken) is null)
        {
            throw new ProductNotFoundException(request.ProductId);
        }

        if (await productVariantRepository.ExistsBySkuAsync(request.Sku, null, cancellationToken))
        {
            throw new DuplicateProductVariantSkuException(request.Sku);
        }

        var productVariant = new ProductVariant(
            request.ProductId,
            request.Sku,
            request.Name,
            request.AttributeSummary,
            request.ImageUrl,
            request.PriceOverride);

        productVariant.SetActive(request.IsActive);

        await productVariantRepository.AddAsync(productVariant, cancellationToken);
        await applicationDbContext.SaveChangesAsync(cancellationToken);
        await applicationCache.InvalidateRegionAsync(CacheRegions.Catalog, cancellationToken);
        await productVectorIndexingService.IndexProductVariantAsync(productVariant.Id, cancellationToken);

        var createdProductVariant = await productVariantRepository.GetByIdAsync(productVariant.Id, cancellationToken)
            ?? throw new ProductVariantNotFoundException(productVariant.Id);

        return mapper.Map<ProductVariantDto>(createdProductVariant);
    }
}
