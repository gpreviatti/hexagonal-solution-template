using System.Diagnostics;
using Infrastructure.Common;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Cache;

public sealed class HybridCacheService(HybridCache cache, ILogger<HybridCacheService> logger)
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<HybridCacheService> _logger = logger;
    private readonly string _className = nameof(HybridCacheService);
    private readonly ActivitySource _activities = DefaultConfigurations.ActivitySource;
    public async ValueTask<TResult> GetOrCreateAsync<TResult>(
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken
    )
    {
        using var activity = _activities.StartActivity($"{_className}.{nameof(GetOrCreateAsync)}");

        Logs.DebugStartingOperation(_logger, $"Key: {key}");

        var result = await _cache.GetOrCreateAsync($"{DefaultConfigurations.ApplicationName}:{key}", factory, cancellationToken: cancellationToken);

        Logs.DebugFinishedOperation(_logger, $"Cache hit: {result != null} for key: {key}");

        activity?.SetTag("key", key);

        return result;
    }

    public async ValueTask CreateAsync<TResult>(string key, TResult value, CancellationToken cancellationToken)
    {
        using var activity = _activities.StartActivity($"{_className}.{nameof(CreateAsync)}");

        Logs.DebugStartingOperation(_logger, $"Key: {key}");

        await _cache.SetAsync($"{DefaultConfigurations.ApplicationName}:{key}", value, cancellationToken: cancellationToken);

        Logs.DebugFinishedOperation(_logger, $"Cached hit: {value != null} for key: {key}");

        activity?.SetTag("key", key);
    }

    public async ValueTask DeleteAsync(string key, CancellationToken cancellationToken)
    {
        using var activity = _activities.StartActivity($"{_className}.{nameof(DeleteAsync)}");

        Logs.DebugStartingOperation(_logger, $"Key: {key}");

        await _cache.RemoveAsync($"{DefaultConfigurations.ApplicationName}:{key}", cancellationToken);

        Logs.DebugFinishedOperation(_logger, $"Cache entry removed for key: {key}");

        activity?.SetTag("key", key);
    }
}
