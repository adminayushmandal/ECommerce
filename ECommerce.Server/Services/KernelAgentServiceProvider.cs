using Domain.Common.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Shared.Enums;

namespace ECommerce.Server.Services
{
    public sealed class KernelAgentServiceProvider : IKernelAgentServiceProvider
    {
        private readonly Kernel _baseKernel = null!;
        private readonly PromptTemplateConfig _productPromptTemplateConfig;
        private readonly KernelPromptTemplateFactory _promptTemplateFactory = new();
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

        public KernelAgentServiceProvider(IConfiguration configuration)
        {
            var ollamaEndpoint = configuration["Enma:OllamaEndpoint"] ?? throw new ApplicationException("Ollama endpoint not configured. Please set 'Enma:OllamaEndpoint' in the configuration.");
            var chatModel = configuration["Enma:ChatModel"] ?? throw new ApplicationException("Chat model not configured. Please set 'Enma:ChatModel' in the configuration.");
            var embeddingModel = configuration["Enma:EmbeddingModel"] ?? throw new ApplicationException("Embedding model not configured. Please set 'Enma:EmbeddingModel' in the configuration.");
            var ollamaHttpClient = new HttpClient()
            {
                BaseAddress = new Uri(ollamaEndpoint)
            };

            _baseKernel = Kernel.CreateBuilder()
               .AddOllamaChatCompletion(chatModel, ollamaHttpClient)
               .AddOllamaEmbeddingGenerator(embeddingModel, ollamaHttpClient)
               .Build();

            _embeddingGenerator = _baseKernel.Services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

            var folder = Path.Combine(Environment.CurrentDirectory, "Infrastructure", "Prompts");

            var pdpTemplate = File.ReadAllText(Path.Combine(folder, "pdp_prompt.txt"))
                ?? throw new FileNotFoundException("System prompt file not found.");

            _productPromptTemplateConfig = new PromptTemplateConfig()
            {
                Template = pdpTemplate,
                Name = "ProductAgentPrompt",
                Description = "Prompt template for product-related queries, including product details, recommendations, and comparisons."
            };
        }

        public ChatCompletionAgent GetAgent(AgentMode mode) =>
            mode switch
            {
                AgentMode.ProductAgent => ProductAgent(_baseKernel.Clone()),
                _ => throw new ApplicationException("Invalid agent mode")
            };

        public async Task<ReadOnlyMemory<float>> GetEmbeddingAsync(string input, CancellationToken ct)
            => await _embeddingGenerator.GenerateVectorAsync(input, cancellationToken: ct);

        private ChatCompletionAgent ProductAgent(Kernel kernel)
        {
            return new(_productPromptTemplateConfig, _promptTemplateFactory)
            {
                Kernel = kernel,
                Arguments = new(new OllamaPromptExecutionSettings()
                {
                    Temperature = 0.5f,
                    TopK = 40,
                    TopP = 0.96f,
                    FunctionChoiceBehavior = FunctionChoiceBehavior.None(),
                    NumPredict = 2000
                })
            };
        }
    }
}
