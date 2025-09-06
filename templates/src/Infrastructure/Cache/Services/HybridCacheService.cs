using Application.Common.Constants;
using Application.Common.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace Infrastructure.Cache.Services;

internal sealed class HybridCacheService(HybridCache cache) : IHybridCacheService
{
    private readonly HybridCache _cache = cache;

    public async ValueTask<TResult?> GetAsync<TResult>(
        string key,
        CancellationToken cancellationToken
    ) => await _cache.GetOrCreateAsync<TResult?>(
        $"{DefaultConfigurations.ApplicationName}:{key}",
        _ => ValueTask.FromResult<TResult?>(default),
        cancellationToken: cancellationToken
    );

    public async ValueTask SetAsync<TValue>(
        string key,
        TValue value,
        CancellationToken cancellationToken
    ) => await _cache.SetAsync($"{DefaultConfigurations.ApplicationName}:{key}", value, cancellationToken: cancellationToken);

    public async ValueTask<TResult> GetOrCreateAsync<TResult>(
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken
    ) => await _cache.GetOrCreateAsync($"{DefaultConfigurations.ApplicationName}:{key}", factory, cancellationToken: cancellationToken);
}
