using Application.Common.Exceptions;
using Application.Common.Caching;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.ProductVariants.Commands;

public sealed record DeleteProductVariantCommand(string ProductId, string ProductVariantId) : IRequest<bool>;

public sealed class DeleteProductVariantCommandHandler(
    IProductVariantRepository productVariantRepository,
    IApplicationDbContext applicationDbContext,
    IProductVectorIndexingService productVectorIndexingService,
    IApplicationCache applicationCache)
    : IRequestHandler<DeleteProductVariantCommand, bool>
{
    public async Task<bool> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
    {
        var productVariant = await productVariantRepository.GetByIdAsync(request.ProductId, request.ProductVariantId, cancellationToken)
            ?? throw new ProductVariantNotFoundException(request.ProductVariantId);

        if (!string.Equals(productVariant.ProductId, request.ProductId, StringComparison.Ordinal))
        {
            throw new ProductVariantProductMismatchException(request.ProductId, request.ProductVariantId);
        }

        productVariantRepository.Remove(productVariant);
        await applicationDbContext.SaveChangesAsync(cancellationToken);
        await applicationCache.InvalidateRegionAsync(CacheRegions.Catalog, cancellationToken);
        await productVectorIndexingService.DeleteProductVariantAsync(productVariant.Id, cancellationToken);

        return true;
    }
}
