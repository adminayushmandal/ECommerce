using Application.Common.Exceptions;
using Application.Common.Caching;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Products.Commands;

public sealed record DeleteProductCommand(string ProductId) : IRequest<bool>;

public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IApplicationDbContext applicationDbContext,
    IProductVectorIndexingService productVectorIndexingService,
    IApplicationCache applicationCache)
    : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        var productVariantIds = product.Variants
            .Select(variant => variant.Id)
            .ToArray();

        productRepository.Remove(product);
        await applicationDbContext.SaveChangesAsync(cancellationToken);
        await applicationCache.InvalidateRegionAsync(CacheRegions.Catalog, cancellationToken);
        await productVectorIndexingService.DeleteProductAsync(product.Id, productVariantIds, cancellationToken);

        return true;
    }
}
