using Application.Common.Constants;
using Application.Common.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace Infrastructure.Cache.Services;

internal sealed class HybridCacheService(HybridCache cache) : IHybridCacheService
{
    private readonly HybridCache _cache = cache;

    public async ValueTask<TResult> GetOrCreateAsync<TResult>(
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken
    ) => await _cache.GetOrCreateAsync($"{DefaultConfigurations.ApplicationName}:{key}", factory, cancellationToken: cancellationToken);

    public ValueTask CreateAsync<TResult>(string key, Func<CancellationToken, ValueTask<TResult>> factory, CancellationToken cancellationToken) =>
        _cache.SetAsync($"{DefaultConfigurations.ApplicationName}:{key}", factory, cancellationToken: cancellationToken);

    public ValueTask DeleteAsync(string key, CancellationToken cancellationToken) =>
        _cache.RemoveAsync($"{DefaultConfigurations.ApplicationName}:{key}", cancellationToken);
}
