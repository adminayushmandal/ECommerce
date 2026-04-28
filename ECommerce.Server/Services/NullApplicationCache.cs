using Application.Common.Interfaces;

namespace ECommerce.Server.Services;

internal sealed class NullApplicationCache : IApplicationCache
{
    public Task<T> GetOrCreateAsync<T>(
        string region,
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken)
    {
        return factory(cancellationToken);
    }

    public Task InvalidateRegionAsync(string region, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
