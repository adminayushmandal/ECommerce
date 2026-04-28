namespace Application.Common.Interfaces;

public interface IApplicationCache
{
    Task<T> GetOrCreateAsync<T>(
        string region,
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken);

    Task InvalidateRegionAsync(string region, CancellationToken cancellationToken);
}
