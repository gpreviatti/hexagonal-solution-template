using Application.Common.Constants;
using Application.Common.Services;
using Infrastructure.Common;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Cache.Services;

internal sealed class HybridCacheService(HybridCache cache, ILogger<HybridCacheService> logger) : IHybridCacheService
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<HybridCacheService> _logger = logger;

    public async ValueTask<TResult> GetOrCreateAsync<TResult>(
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken
    )
    {
        Logs.RetrievingCacheEntry(_logger, key);

        var result = await _cache.GetOrCreateAsync($"{DefaultConfigurations.ApplicationName}:{key}", factory, cancellationToken: cancellationToken);

        Logs.CacheEntryRetrieved(_logger, key);

        return result;
    }

    public async ValueTask CreateAsync<TResult>(string key, TResult value, CancellationToken cancellationToken)
    {
        Logs.CreatingCacheEntry(_logger, key);

        await _cache.SetAsync($"{DefaultConfigurations.ApplicationName}:{key}", value, cancellationToken: cancellationToken);

        Logs.CacheEntryCreated(_logger, key);
    }

    public async ValueTask DeleteAsync(string key, CancellationToken cancellationToken)
    {
        Logs.DeletingCacheEntry(_logger, key);

        await _cache.RemoveAsync($"{DefaultConfigurations.ApplicationName}:{key}", cancellationToken);

        Logs.CacheEntryDeleted(_logger, key);
    }
}
