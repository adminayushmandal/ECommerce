using Application.Common.Interfaces;
using Domain.Common.Interfaces;
using Domain.Entities;
using Domain.Entities.Vector;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PgvectorVector = Pgvector.Vector;

namespace Infrastructure.Services
{
    internal sealed class ProductVectorIndexingService(
        ApplicationDbContext context,
        IKernelAgentServiceProvider kernelAgentServiceProvider,
        ILogger<ProductVectorIndexingService> logger) : IProductVectorIndexingService
    {
        internal const string ProductRecordType = "product";
        internal const string VariantRecordType = "product_variant";

        public async Task DeleteProductAsync(string productId, IEnumerable<string> productVariantIds, CancellationToken cancellationToken)
        {
            var recordIds = productVariantIds
                .Select(CreateProductVariantIndexId)
                .Prepend(CreateProductIndexId(productId))
                .ToArray();

            var records = await context.ProductVectorRecords
                .Where(record => recordIds.Contains(record.Id))
                .ToListAsync(cancellationToken);

            if (records.Count == 0)
            {
                return;
            }

            context.ProductVectorRecords.RemoveRange(records);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Deleted product '{ProductId}' and {VariantCount} related vector records.",
                productId,
                records.Count - 1);
        }

        public async Task DeleteProductVariantAsync(string productVariantId, CancellationToken cancellationToken)
        {
            var record = await context.ProductVectorRecords
                .FirstOrDefaultAsync(x => x.Id == CreateProductVariantIndexId(productVariantId), cancellationToken);

            if (record is null)
            {
                return;
            }

            context.ProductVectorRecords.Remove(record);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Vector record deleted successfully for variant '{ProductVariantId}'.", productVariantId);
        }

        public async Task IndexProductGraphAsync(string productId, CancellationToken cancellationToken)
        {
            var product = await context.Products
                .Include(x => x.Category)
                .Include(x => x.Variants)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken)
                ?? throw new InvalidOperationException($"Failed to get the product - {productId}");

            var records = new List<ProductVectorRecord>(1 + product.Variants.Count)
            {
                await BuildProductVectorRecordAsync(product, cancellationToken)
            };

            foreach (var variant in product.Variants)
            {
                records.Add(await BuildVariantRecordAsync(product, variant, cancellationToken));
            }

            await UpsertRangeAsync(records, cancellationToken);

            var expectedIds = records.Select(record => record.Id).ToHashSet(StringComparer.Ordinal);
            var obsoleteRecords = await context.ProductVectorRecords
                .Where(record => record.ProductId == productId && !expectedIds.Contains(record.Id))
                .ToListAsync(cancellationToken);

            if (obsoleteRecords.Count > 0)
            {
                context.ProductVectorRecords.RemoveRange(obsoleteRecords);
                await context.SaveChangesAsync(cancellationToken);
            }

            logger.LogInformation("Indexed product graph successfully for '{ProductId}'.", productId);
        }

        public async Task IndexProductVariantAsync(string productVariantId, CancellationToken cancellationToken)
        {
            var variant = await context.ProductVariants
                .AsNoTracking()
                .Include(x => x.Product)
                .ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == productVariantId, cancellationToken)
                ?? throw new InvalidOperationException($"Failed to find the variant - {productVariantId}");

            var record = await BuildVariantRecordAsync(variant.Product, variant, cancellationToken);
            await UpsertRangeAsync([record], cancellationToken);

            logger.LogInformation("Variant vector record added successfully for '{ProductVariantId}'.", productVariantId);
        }

        public async Task RebuildCatalogIndexAsync(CancellationToken cancellationToken)
        {
            var products = await context.Products
                .AsNoTracking()
                .Include(x => x.Variants)
                .Include(x => x.Category)
                .ToListAsync(cancellationToken);

            foreach (var product in products)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await IndexProductGraphAsync(product.Id, cancellationToken);
            }
        }

        private async Task<ProductVectorRecord> BuildProductVectorRecordAsync(Product product, CancellationToken cancellationToken)
        {
            var content = BuildProductVectorContent(product);
            var embedding = await kernelAgentServiceProvider.GetEmbeddingAsync(content, cancellationToken);

            return new ProductVectorRecord
            {
                Id = CreateProductIndexId(product.Id),
                CategoryName = product.Category.Name,
                Content = product.Description,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                Name = product.Name,
                Price = product.BasePrice,
                ProductId = product.Id,
                RecordType = ProductRecordType,
                Sku = product.Sku,
                Tags =
                [
                    ProductRecordType,
                    product.Sku,
                    product.Slug,
                    product.Id
                ],
                Embedding = new PgvectorVector(embedding.ToArray())
            };
        }

        private async Task<ProductVectorRecord> BuildVariantRecordAsync(Product product, ProductVariant productVariant, CancellationToken cancellationToken)
        {
            var content = BuildVariantContent(product, productVariant);
            var embedding = await kernelAgentServiceProvider.GetEmbeddingAsync(content, cancellationToken);

            return new ProductVectorRecord
            {
                Id = CreateProductVariantIndexId(productVariant.Id),
                CategoryName = product.Category.Name,
                Name = productVariant.Name,
                Content = product.Description,
                ImageUrl = productVariant.ImageUrl,
                IsActive = productVariant.IsActive,
                Price = productVariant.PriceOverride ?? product.BasePrice,
                ProductId = product.Id,
                ProductVariantId = productVariant.Id,
                RecordType = VariantRecordType,
                Sku = productVariant.Sku,
                Tags =
                [
                    VariantRecordType,
                    productVariant.Id,
                    product.Category.Name,
                    product.Sku
                ],
                Embedding = new PgvectorVector(embedding.ToArray())
            };
        }

        private static string BuildProductVectorContent(Product product) =>
            string.Join(
                Environment.NewLine,
                [
                    $"Id = {product.Id}",
                    $"Name = {product.Name}",
                    $"Sku = {product.Sku}",
                    $"Description = {product.Description}",
                    $"BasePrice = {product.BasePrice}",
                    $"Images = {product.ImageUrl}",
                    $"IsActive = {product.IsActive}",
                    $"Category = {product.Category.Name}"
                ]);

        private static string BuildVariantContent(Product product, ProductVariant productVariant) =>
            string.Join(
                Environment.NewLine,
                [
                    $"Id = {productVariant.Id}",
                    $"Name = {productVariant.Name}",
                    $"Sku = {productVariant.Sku}",
                    $"Description = {product.Description}",
                    $"BasePrice = {productVariant.PriceOverride ?? product.BasePrice}",
                    $"Images = {productVariant.ImageUrl}",
                    $"IsActive = {productVariant.IsActive}",
                    $"Category = {product.Category.Name}",
                    $"Attributes = {productVariant.AttributeSummary ?? "N/A"}"
                ]);

        private static string CreateProductIndexId(string productId) => $"product_{productId}";

        private static string CreateProductVariantIndexId(string variantId) => $"product_variant_{variantId}";

        private async Task UpsertRangeAsync(IReadOnlyCollection<ProductVectorRecord> records, CancellationToken cancellationToken)
        {
            var recordIds = records.Select(record => record.Id).ToArray();
            var existingRecords = await context.ProductVectorRecords
                .Where(record => recordIds.Contains(record.Id))
                .ToDictionaryAsync(record => record.Id, StringComparer.Ordinal, cancellationToken);

            foreach (var record in records)
            {
                if (existingRecords.TryGetValue(record.Id, out var existingRecord))
                {
                    context.Entry(existingRecord).CurrentValues.SetValues(record);
                }
                else
                {
                    await context.ProductVectorRecords.AddAsync(record, cancellationToken);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
