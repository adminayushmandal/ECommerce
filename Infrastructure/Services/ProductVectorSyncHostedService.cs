using Application.Common.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

internal sealed class ProductVectorSyncHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<ProductVectorSyncHostedService> logger) : BackgroundService
{
    private const string ProductRecordPrefix = "product_";
    private const string ProductVariantRecordPrefix = "product_variant_";
    private const int BatchSize = 25;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ProcessPendingProductsAsync(stoppingToken);

        using var timer = new PeriodicTimer(PollInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessPendingProductsAsync(stoppingToken);
        }
    }

    private async Task ProcessPendingProductsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var indexingService = scope.ServiceProvider.GetRequiredService<IProductVectorIndexingService>();

            var productIds = await GetProductsMissingEmbeddingsAsync(context, cancellationToken);
            if (productIds.Count == 0)
            {
                return;
            }

            foreach (var productId in productIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await indexingService.IndexProductGraphAsync(productId, cancellationToken);
            }

            logger.LogInformation(
                "Indexed embeddings for {ProductCount} products missing vector records.",
                productIds.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing pending product embeddings.");
        }
    }

    private static async Task<List<string>> GetProductsMissingEmbeddingsAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var existingRecordIds = await context.ProductVectorRecords
            .Select(record => record.Id)
            .ToListAsync(cancellationToken);

        var existingRecordIdSet = existingRecordIds.ToHashSet(StringComparer.Ordinal);

        var products = await context.Products
            .AsNoTracking()
            .Include(product => product.Variants)
            .OrderBy(product => product.CreatedAt)
            .ToListAsync(cancellationToken);

        return products
            .Where(product =>
                !existingRecordIdSet.Contains(CreateProductRecordId(product.Id)) ||
                product.Variants.Any(variant =>
                    !existingRecordIdSet.Contains(CreateProductVariantRecordId(variant.Id))))
            .Select(product => product.Id)
            .Take(BatchSize)
            .ToList();
    }

    private static string CreateProductRecordId(string productId) => ProductRecordPrefix + productId;

    private static string CreateProductVariantRecordId(string productVariantId) => ProductVariantRecordPrefix + productVariantId;
}
