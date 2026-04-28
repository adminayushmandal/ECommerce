namespace Application.Common.Caching;

public static class CacheRegions
{
    public const string Catalog = "catalog";
    public const string Stores = "stores";
}

public static class CacheDurations
{
    public static readonly TimeSpan CatalogProducts = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan StoreCatalog = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan ProductDetails = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan ProductAvailability = TimeSpan.FromSeconds(45);
    public static readonly TimeSpan Stores = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan NearestStore = TimeSpan.FromMinutes(10);
}
