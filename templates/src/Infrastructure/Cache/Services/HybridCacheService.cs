using Application.Common.Constants;
using Application.Common.Services;
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
        _logger.LogDebug("[HybridCacheService] | [GetOrCreateAsync] | [{Key}] | Retrieving cache entry", key);

        var result = await _cache.GetOrCreateAsync($"{DefaultConfigurations.ApplicationName}:{key}", factory, cancellationToken: cancellationToken);

        _logger.LogDebug("[HybridCacheService] | [GetOrCreateAsync] | [{Key}] | Cache entry retrieved", key);

        return result;
    }

    public async ValueTask CreateAsync<TResult>(string key, TResult value, CancellationToken cancellationToken)
    {
        _logger.LogDebug("[HybridCacheService] | [CreateAsync] | [{Key}] | Creating cache entry", key);

        await _cache.SetAsync($"{DefaultConfigurations.ApplicationName}:{key}", value, cancellationToken: cancellationToken);

        _logger.LogDebug("[HybridCacheService] | [CreateAsync] | [{Key}] | Cache entry created", key);
    }

    public async ValueTask DeleteAsync(string key, CancellationToken cancellationToken)
    {
        _logger.LogDebug("[HybridCacheService] | [DeleteAsync] | [{Key}] | Deleting cache entry", key);

        await _cache.RemoveAsync($"{DefaultConfigurations.ApplicationName}:{key}", cancellationToken);

        _logger.LogDebug("[HybridCacheService] | [DeleteAsync] | [{Key}] | Cache entry deleted", key);
    }
}
