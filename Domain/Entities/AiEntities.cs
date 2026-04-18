namespace Domain.Entities;

public sealed class AiConversationSession : BaseAuditableEntity
{
    public string? UserId { get; private set; }
    public User? User { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public ICollection<AiConversationMessage> Messages { get; private set; } = [];

    private AiConversationSession()
    {
    }

    public AiConversationSession(string? userId, string title, string model)
    {
        UserId = userId;
        Title = title;
        Model = model;
    }

    public void Rename(string title)
    {
        Title = title;
    }

    public void ChangeModel(string model)
    {
        Model = model;
    }

    public void Close()
    {
        IsActive = false;
    }

    public void Reopen()
    {
        IsActive = true;
    }
}

public sealed class AiConversationMessage : BaseAuditableEntity
{
    public string SessionId { get; private set; } = string.Empty;
    public AiConversationSession Session { get; private set; } = default!;
    public AiChatMessageRole Role { get; private set; }
    public int SequenceNumber { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? ProviderMessageId { get; private set; }
    public string? Model { get; private set; }
    public int? PromptTokens { get; private set; }
    public int? CompletionTokens { get; private set; }
    public bool IsError { get; private set; }

    private AiConversationMessage()
    {
    }

    public AiConversationMessage(
        string sessionId,
        AiChatMessageRole role,
        int sequenceNumber,
        string content,
        string? model = null,
        string? providerMessageId = null)
    {
        SessionId = sessionId;
        Role = role;
        SequenceNumber = sequenceNumber;
        Content = content;
        Model = model;
        ProviderMessageId = providerMessageId;
    }

    public void UpdateContent(string content)
    {
        Content = content;
    }

    public void SetProviderMessageId(string? providerMessageId)
    {
        ProviderMessageId = providerMessageId;
    }

    public void SetTokenUsage(int? promptTokens, int? completionTokens)
    {
        PromptTokens = promptTokens;
        CompletionTokens = completionTokens;
    }

    public void MarkAsError()
    {
        IsError = true;
    }
}

public sealed class AiKnowledgeDocument : BaseAuditableEntity
{
    public AiDocumentSourceType SourceType { get; private set; }
    public string SourceKey { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? SourceUri { get; private set; }
    public string? ContentHash { get; private set; }
    public string? MetadataJson { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<AiKnowledgeChunk> Chunks { get; private set; } = [];

    private AiKnowledgeDocument()
    {
    }

    public AiKnowledgeDocument(
        AiDocumentSourceType sourceType,
        string sourceKey,
        string title,
        string content,
        string? sourceUri = null,
        string? contentHash = null,
        string? metadataJson = null)
    {
        SourceType = sourceType;
        SourceKey = sourceKey;
        Title = title;
        Content = content;
        SourceUri = sourceUri;
        ContentHash = contentHash;
        MetadataJson = metadataJson;
    }

    public void UpdateContent(
        string title,
        string content,
        string? sourceUri = null,
        string? contentHash = null,
        string? metadataJson = null)
    {
        Title = title;
        Content = content;
        SourceUri = sourceUri;
        ContentHash = contentHash;
        MetadataJson = metadataJson;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}

public sealed class AiKnowledgeChunk : BaseAuditableEntity
{
    public string DocumentId { get; private set; } = string.Empty;
    public AiKnowledgeDocument Document { get; private set; } = default!;
    public int SequenceNumber { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? ContentHash { get; private set; }
    public int CharacterCount { get; private set; }
    public bool IsEmbedded { get; private set; }
    public ICollection<AiEmbedding> Embeddings { get; private set; } = [];

    private AiKnowledgeChunk()
    {
    }

    public AiKnowledgeChunk(
        string documentId,
        int sequenceNumber,
        string content,
        string? contentHash = null)
    {
        DocumentId = documentId;
        SequenceNumber = sequenceNumber;
        Content = content;
        ContentHash = contentHash;
        CharacterCount = content.Length;
    }

    public void UpdateContent(string content, string? contentHash = null)
    {
        Content = content;
        ContentHash = contentHash;
        CharacterCount = content.Length;
        IsEmbedded = false;
    }

    public void MarkEmbedded()
    {
        IsEmbedded = true;
    }
}

public sealed class AiEmbedding : BaseAuditableEntity
{
    public string? DocumentChunkId { get; private set; }
    public AiKnowledgeChunk? DocumentChunk { get; private set; }
    public string? ProductId { get; private set; }
    public Product? Product { get; private set; }
    public string? ProductVariantId { get; private set; }
    public ProductVariant? ProductVariant { get; private set; }
    public string Model { get; private set; } = string.Empty;
    public int Dimensions { get; private set; }
    public string SourceText { get; private set; } = string.Empty;
    public string? ContentHash { get; private set; }
    public float[] Vector { get; private set; } = [];
    public bool IsActive { get; private set; } = true;

    private AiEmbedding()
    {
    }

    public AiEmbedding(
        string model,
        float[] vector,
        string sourceText,
        string? contentHash = null,
        string? documentChunkId = null,
        string? productId = null,
        string? productVariantId = null)
    {
        DocumentChunkId = documentChunkId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        Model = model;
        SourceText = sourceText;
        ContentHash = contentHash;
        ReplaceVector(vector);
    }

    public void ReplaceVector(float[] vector)
    {
        Vector = vector.ToArray();
        Dimensions = Vector.Length;
    }

    public void UpdateSource(string sourceText, string? contentHash = null)
    {
        SourceText = sourceText;
        ContentHash = contentHash;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
