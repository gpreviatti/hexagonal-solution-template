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
}
