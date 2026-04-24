using Application.Features.Enma.Queries.GetProductDetailByEnma;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace ECommerce.Server.Hubs
{
    public sealed class EnmaHub(ISender sender) : Hub
    {
        public async IAsyncEnumerable<EnmaStreamChunk> StreamProductDetail(
            string userQuery,
            string? productId,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var chunk in sender.CreateStream(
                new StreamProductDetailByEnmaQuery(userQuery, productId),
                cancellationToken))
            {
                yield return chunk;
            }
        }
    }
}
