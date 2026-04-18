namespace Application.Common.Exceptions;

public sealed class CategoryNotFoundException(string categoryId)
    : BusinessLogicException($"Category '{categoryId}' was not found.", 404)
{
    public string CategoryId { get; } = categoryId;
}

public sealed class ProductNotFoundException(string productId)
    : BusinessLogicException($"Product '{productId}' was not found.", 404)
{
    public string ProductId { get; } = productId;
}

public sealed class ProductVariantNotFoundException(string productVariantId)
    : BusinessLogicException($"Product variant '{productVariantId}' was not found.", 404)
{
    public string ProductVariantId { get; } = productVariantId;
}

public sealed class DuplicateProductSkuException(string sku)
    : BusinessLogicException($"A product with SKU '{sku}' already exists.", 409)
{
    public string Sku { get; } = sku;
}

public sealed class DuplicateProductSlugException(string slug)
    : BusinessLogicException($"A product with slug '{slug}' already exists.", 409)
{
    public string Slug { get; } = slug;
}

public sealed class DuplicateProductVariantSkuException(string sku)
    : BusinessLogicException($"A product variant with SKU '{sku}' already exists.", 409)
{
    public string Sku { get; } = sku;
}

public sealed class ProductVariantProductMismatchException(string productId, string productVariantId)
    : BusinessLogicException($"Product variant '{productVariantId}' does not belong to product '{productId}'.", 400)
{
    public string ProductId { get; } = productId;
    public string ProductVariantId { get; } = productVariantId;
}

public sealed class ProductVariantSelectionRequiredException(string productId)
    : BusinessLogicException($"A product variant must be selected for product '{productId}'.", 400)
{
    public string ProductId { get; } = productId;
}
