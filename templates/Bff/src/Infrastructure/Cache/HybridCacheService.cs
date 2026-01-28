using Infrastructure.Common;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Cache;

public sealed partial class HybridCacheService(HybridCache cache, ILogger<HybridCacheService> logger)
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<HybridCacheService> _logger = logger;

    public async ValueTask<TResult> GetOrCreateAsync<TResult>(
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken
    )
    {
        RetrievingCacheEntry(_logger, key);

        var result = await _cache.GetOrCreateAsync($"{DefaultConfigurations.ApplicationName}:{key}", factory, cancellationToken: cancellationToken);

        CacheEntryRetrieved(_logger, key);

        return result;
    }

    public async ValueTask CreateAsync<TResult>(string key, TResult value, CancellationToken cancellationToken)
    {
        CreatingCacheEntry(_logger, key);

        await _cache.SetAsync($"{DefaultConfigurations.ApplicationName}:{key}", value, cancellationToken: cancellationToken);

        CacheEntryCreated(_logger, key);
    }

    public async ValueTask DeleteAsync(string key, CancellationToken cancellationToken)
    {
        DeletingCacheEntry(_logger, key);

        await _cache.RemoveAsync($"{DefaultConfigurations.ApplicationName}:{key}", cancellationToken);

        CacheEntryDeleted(_logger, key);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "[HybridCacheService] | [GetOrCreateAsync] | [{Key}] | Retrieving cache entry"
    )]
    private static partial void RetrievingCacheEntry(ILogger logger, string key);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "[HybridCacheService] | [GetOrCreateAsync] | [{Key}] | Cache entry retrieved"
    )]
    private static partial void CacheEntryRetrieved(ILogger logger, string key);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "[HybridCacheService] | [CreateAsync] | [{Key}] | Creating cache entry"
    )]
    private static partial void CreatingCacheEntry(ILogger logger, string key);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "[HybridCacheService] | [CreateAsync] | [{Key}] | Cache entry created"
    )]
    private static partial void CacheEntryCreated(ILogger logger, string key);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "[HybridCacheService] | [DeleteAsync] | [{Key}] | Deleting cache entry"
    )]
    private static partial void DeletingCacheEntry(ILogger logger, string key);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "[HybridCacheService] | [DeleteAsync] | [{Key}] | Cache entry deleted"
    )]
    private static partial void CacheEntryDeleted(ILogger logger, string key);
}
