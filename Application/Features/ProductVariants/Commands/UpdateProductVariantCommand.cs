using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using MediatR;

namespace Application.Features.ProductVariants.Commands;

public sealed record UpdateProductVariantCommand(
    string ProductId,
    string ProductVariantId,
    string Sku,
    string Name,
    string? AttributeSummary,
    string ImageUrl,
    decimal? PriceOverride,
    bool IsActive) : IRequest<ProductVariantDto>;

public sealed class UpdateProductVariantCommandHandler(
    IProductRepository productRepository,
    IProductVariantRepository productVariantRepository,
    IApplicationDbContext applicationDbContext,
    IProductVectorIndexingService productVectorIndexingService,
    IMapper mapper)
    : IRequestHandler<UpdateProductVariantCommand, ProductVariantDto>
{
    public async Task<ProductVariantDto> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
    {
        if (await productRepository.GetByIdAsync(request.ProductId, cancellationToken) is null)
        {
            throw new ProductNotFoundException(request.ProductId);
        }

        var productVariant = await productVariantRepository.GetByIdAsync(request.ProductId, request.ProductVariantId, cancellationToken)
            ?? throw new ProductVariantNotFoundException(request.ProductVariantId);

        if (!string.Equals(productVariant.ProductId, request.ProductId, StringComparison.Ordinal))
        {
            throw new ProductVariantProductMismatchException(request.ProductId, request.ProductVariantId);
        }

        if (await productVariantRepository.ExistsBySkuAsync(request.Sku, request.ProductVariantId, cancellationToken))
        {
            throw new DuplicateProductVariantSkuException(request.Sku);
        }

        productVariant.UpdateDetails(
            request.Sku,
            request.Name,
            request.AttributeSummary,
            request.ImageUrl,
            request.PriceOverride);

        productVariant.SetActive(request.IsActive);

        await applicationDbContext.SaveChangesAsync(cancellationToken);
        await productVectorIndexingService.IndexProductVariantAsync(productVariant.Id, cancellationToken);

        return mapper.Map<ProductVariantDto>(productVariant);
    }
}
