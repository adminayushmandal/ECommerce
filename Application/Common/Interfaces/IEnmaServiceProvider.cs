using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace Application.Common.Interfaces
{
    public interface IEnmaServiceProvider
    {
        Kernel ChatService { get; }
        ChatCompletionAgent Agent { get; }
    }
}
