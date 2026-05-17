using System.Diagnostics;
using System.Globalization;
using Application.Common.Helpers;
using Application.Common.Services;
using Domain.Common;
using Domain.Common.Extensions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Cache.Services;

internal sealed class HybridCacheService(HybridCache cache, ILogger<HybridCacheService> logger) : IHybridCacheService
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<HybridCacheService> _logger = logger;
    private readonly string _className = nameof(HybridCacheService);
    private readonly ActivitySource _activities = DefaultConfigurations.ActivitySource;
    private readonly string _cacheHitMessage = "Cache hit: {0} for key: {1}";
    private readonly string _cacheRemovedMessage = "Cache entry removed for key: {0}";
    public async ValueTask<TResult> GetOrCreateAsync<TResult>(
        Guid correlationId,
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken
    )
    {
        using var activity = _activities.StartActivity($"{_className}.{nameof(GetOrCreateAsync)}");
        activity.SetDefaultTags();

        Logs.DebugStartingOperation(_logger, correlationId, key);
        var result = await _cache.GetOrCreateAsync($"{DefaultConfigurations.ApplicationName}:{key}", factory, cancellationToken: cancellationToken);

        Logs.DebugFinishedOperation(_logger, correlationId, string.Format(CultureInfo.InvariantCulture, _cacheHitMessage, result, key));

        activity?.SetTag("key", key);

        return result;
    }

    public async ValueTask CreateAsync<TResult>(Guid correlationId, string key, TResult value, CancellationToken cancellationToken)
    {
        using var activity = _activities.StartActivity($"{_className}.{nameof(CreateAsync)}");
        activity.SetDefaultTags();

        Logs.DebugStartingOperation(_logger, correlationId, key);

        await _cache.SetAsync($"{DefaultConfigurations.ApplicationName}:{key}", value, cancellationToken: cancellationToken);

        Logs.DebugFinishedOperation(_logger, correlationId, $"Cached hit: {value} for key: {key}");

        activity?.SetTag("key", key);
    }

    public async ValueTask DeleteAsync(Guid correlationId, string key, CancellationToken cancellationToken)
    {
        using var activity = _activities.StartActivity($"{_className}.{nameof(DeleteAsync)}");
        activity.SetDefaultTags();

        Logs.DebugStartingOperation(_logger, correlationId, key);

        await _cache.RemoveAsync($"{DefaultConfigurations.ApplicationName}:{key}", cancellationToken);

        Logs.DebugFinishedOperation(_logger, correlationId, $"Cache entry removed for key: {key}");

        activity?.SetTag("key", key);
    }
}
