using Application.Common.Constants;
using Application.Common.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace Infrastructure.Cache.Services;

internal sealed class HybridCacheService(HybridCache cache) : IHybridCacheService
{
    private readonly HybridCache _cache = cache;

    public async Task<TResult> GetOrCreateAsync<TResult>(
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken = default
    ) => await _cache.GetOrCreateAsync($"{DefaultConfigurations.ApplicationName}:{key}", factory, cancellationToken: cancellationToken);
}
