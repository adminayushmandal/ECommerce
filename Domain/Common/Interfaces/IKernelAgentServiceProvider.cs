using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Agents;
using Shared.Enums;

namespace Domain.Common.Interfaces
{
    public interface IKernelAgentServiceProvider
    {
        ChatCompletionAgent GetAgent(AgentMode mode);
        Task<ReadOnlyMemory<float>> GetEmbeddingAsync(string input, CancellationToken ct);
    }
}
