namespace Application.Common.Interfaces;

public interface IProductVectorIndexingService
{
    Task RebuildCatalogIndexAsync(CancellationToken cancellationToken);
    Task IndexProductGraphAsync(string productId, CancellationToken cancellationToken);
    Task IndexProductVariantAsync(string productVariantId, CancellationToken cancellationToken);
    Task DeleteProductAsync(string productId, IEnumerable<string> productVariantIds, CancellationToken cancellationToken);
    Task DeleteProductVariantAsync(string productVariantId, CancellationToken cancellationToken);
}
