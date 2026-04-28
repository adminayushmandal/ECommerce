namespace Infrastructure.Services;

internal sealed class PayPalSdkOptions
{
    public const string SectionName = "PayPalSdk";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Environment { get; set; } = "Sandbox";
    public string CurrencyCode { get; set; } = "USD";
    public string BrandName { get; set; } = "ECommerce";
    public string Locale { get; set; } = "en-IN";
}
