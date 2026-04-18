namespace Domain.Enums;

public enum AiDocumentSourceType
{
    Unknown = 0,
    Product = 1,
    ProductVariant = 2,
    Faq = 3,
    Policy = 4,
    Guide = 5,
    UserQuery = 6
}

public enum AiChatMessageRole
{
    System = 0,
    User = 1,
    Assistant = 2,
    Tool = 3
}
