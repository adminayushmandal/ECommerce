using Application.Common.Interfaces;
using StackExchange.Redis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ECommerce.Server.Services;

internal sealed class RedisApplicationCache(
    IConnectionMultiplexer redis,
    ILogger<RedisApplicationCache> logger) : IApplicationCache
{
    private const string KeyPrefix = "ecommerce";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<T> GetOrCreateAsync<T>(
        string region,
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(region);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        if (ttl <= TimeSpan.Zero)
        {
            return await factory(cancellationToken);
        }

        var database = redis.GetDatabase();
        var cacheKey = await CreateCacheKeyAsync(database, region, key);

        var cachedValue = await TryReadAsync<T>(database, cacheKey);
        if (cachedValue.Found)
        {
            return cachedValue.Value!;
        }

        var value = await factory(cancellationToken);
        await TryWriteAsync(database, cacheKey, value, ttl);

        return value;
    }

    public async Task InvalidateRegionAsync(string region, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(region);

        try
        {
            var versionKey = CreateRegionVersionKey(region);
            await redis.GetDatabase().StringIncrementAsync(versionKey);
            logger.LogInformation("Invalidated application cache region '{CacheRegion}'.", region);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Redis cache invalidation failed for region '{CacheRegion}'.", region);
        }
        catch (TimeoutException exception)
        {
            logger.LogWarning(exception, "Redis cache invalidation timed out for region '{CacheRegion}'.", region);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis cache invalidation failed unexpectedly for region '{CacheRegion}'.", region);
        }
    }

    private async Task<string> CreateCacheKeyAsync(IDatabase database, string region, string logicalKey)
    {
        var version = await GetRegionVersionAsync(database, region);
        var hashedKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(logicalKey))).ToLowerInvariant();
        return $"{KeyPrefix}:{NormalizeRegion(region)}:v{version}:{hashedKey}";
    }

    private async Task<string> GetRegionVersionAsync(IDatabase database, string region)
    {
        var versionKey = CreateRegionVersionKey(region);

        try
        {
            var version = await database.StringGetAsync(versionKey);
            if (version.HasValue)
            {
                return version.ToString();
            }

            var initialVersion = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
            await database.StringSetAsync(versionKey, initialVersion);
            return initialVersion;
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Redis cache version lookup failed for region '{CacheRegion}'.", region);
            return "unavailable";
        }
        catch (TimeoutException exception)
        {
            logger.LogWarning(exception, "Redis cache version lookup timed out for region '{CacheRegion}'.", region);
            return "unavailable";
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis cache version lookup failed unexpectedly for region '{CacheRegion}'.", region);
            return "unavailable";
        }
    }

    private async Task<(bool Found, T? Value)> TryReadAsync<T>(IDatabase database, string cacheKey)
    {
        try
        {
            var cachedValue = await database.StringGetAsync(cacheKey);
            if (!cachedValue.HasValue)
            {
                return (false, default);
            }

            var value = JsonSerializer.Deserialize<T>(cachedValue.ToString(), SerializerOptions);
            return value is null ? (false, default) : (true, value);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Cached value for key '{CacheKey}' could not be deserialized.", cacheKey);
            return (false, default);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Redis cache read failed for key '{CacheKey}'.", cacheKey);
            return (false, default);
        }
        catch (TimeoutException exception)
        {
            logger.LogWarning(exception, "Redis cache read timed out for key '{CacheKey}'.", cacheKey);
            return (false, default);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis cache read failed unexpectedly for key '{CacheKey}'.", cacheKey);
            return (false, default);
        }
    }

    private async Task TryWriteAsync<T>(IDatabase database, string cacheKey, T value, TimeSpan ttl)
    {
        try
        {
            var payload = JsonSerializer.Serialize(value, SerializerOptions);
            await database.StringSetAsync(cacheKey, payload, ttl);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Redis cache write failed for key '{CacheKey}'.", cacheKey);
        }
        catch (TimeoutException exception)
        {
            logger.LogWarning(exception, "Redis cache write timed out for key '{CacheKey}'.", cacheKey);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis cache write failed unexpectedly for key '{CacheKey}'.", cacheKey);
        }
    }

    private static string CreateRegionVersionKey(string region)
    {
        return $"{KeyPrefix}:version:{NormalizeRegion(region)}";
    }

    private static string NormalizeRegion(string region)
    {
        return region.Trim().ToLowerInvariant();
    }
}
