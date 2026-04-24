using Application.Common.Interfaces;
using Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Pgvector.EntityFrameworkCore;
using PgvectorVector = Pgvector.Vector;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Application.Features.Enma.Queries.GetProductDetailByEnma
{
    public sealed record StreamProductDetailByEnmaQuery(string UserQuery, string? ProductId = null) : IStreamRequest<EnmaStreamChunk>;

    public sealed record EnmaStreamChunk(string Type, string? Content = null);

    internal sealed class GetProductDetailByEnmaQueryHandler(
        IKernelAgentServiceProvider kernelAgentServiceProvider,
        IApplicationDbContext dbContext)
        :           IStreamRequestHandler<StreamProductDetailByEnmaQuery, EnmaStreamChunk>
    {
        private const string DeltaChunkType = "delta";
        private const string CompletedChunkType = "completed";
        private const string FallbackResponse = "I couldn't find relevant information for that request.";

        public IAsyncEnumerable<EnmaStreamChunk> Handle(
            StreamProductDetailByEnmaQuery request,
            CancellationToken cancellationToken)
            => StreamResponseAsync(request.UserQuery, request.ProductId, cancellationToken);

        private async IAsyncEnumerable<EnmaStreamChunk> StreamResponseAsync(
            string userQuery,
            string? productId,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var productRecords = await QueryRelevantRecordsAsync(userQuery, productId, cancellationToken);

            if (productRecords.Count == 0)
            {
                yield return new EnmaStreamChunk(DeltaChunkType, FallbackResponse);
                yield return new EnmaStreamChunk(CompletedChunkType);
                yield break;
            }

            var agent = kernelAgentServiceProvider.GetAgent(Shared.Enums.AgentMode.ProductAgent);
            var agentInvokeOptions = new AgentInvokeOptions
            {
                KernelArguments = new KernelArguments(agent.Arguments!)
                {
                    ["userQuery"] = userQuery,
                    ["context"] = SerializeContext(productRecords),
                },
            };

            await foreach (StreamingChatMessageContent message in agent.InvokeStreamingAsync(
                options: agentInvokeOptions,
                cancellationToken: cancellationToken))
            {
                if (!string.IsNullOrEmpty(message.Content))
                {
                    yield return new EnmaStreamChunk(DeltaChunkType, message.Content);
                }
            }

            yield return new EnmaStreamChunk(CompletedChunkType);
        }

        private async Task<List<Domain.Entities.Vector.ProductVectorRecord>> QueryRelevantRecordsAsync(
            string userQuery,
            string? productId,
            CancellationToken cancellationToken)
        {
            var queryEmbedding =
                new PgvectorVector((await kernelAgentServiceProvider.GetEmbeddingAsync(userQuery, cancellationToken)).ToArray());

            var query = dbContext.ProductVectorRecords
                .Where(x => x.IsActive)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(productId))
            {
                query = query.Where(x => x.ProductId == productId);
            }

            return await query
                .OrderBy(x => x.Embedding.CosineDistance(queryEmbedding))
                .Take(3)
                .ToListAsync(cancellationToken);
        }

        private static string SerializeContext(IEnumerable<Domain.Entities.Vector.ProductVectorRecord> records)
            => JsonSerializer.Serialize(records.Select(rec => new
            {
                rec.Id,
                rec.ProductId,
                rec.ProductVariantId,
                rec.Name,
                Description = rec.Content,
                rec.Price,
                Category = rec.CategoryName,
                rec.Sku,
                rec.ImageUrl,
            }));
    }
}
