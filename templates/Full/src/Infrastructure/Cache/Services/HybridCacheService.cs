using System.Diagnostics;
using Application.Common.Constants;
using Application.Common.Helpers;
using Application.Common.Services;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Cache.Services;

internal sealed class HybridCacheService(HybridCache cache, ILogger<HybridCacheService> logger) : IHybridCacheService
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<HybridCacheService> _logger = logger;
    private readonly string _className = nameof(HybridCacheService);
    private readonly Stopwatch _stopwatch = new();

    public async ValueTask<TResult> GetOrCreateAsync<TResult>(
        Guid correlationId,
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken
    )
    {
        _stopwatch.Restart();

        Logs.DebugStartingOperation(_logger, _className, nameof(GetOrCreateAsync), correlationId, key);
        
        var result = await _cache.GetOrCreateAsync($"{DefaultConfigurations.ApplicationName}:{key}", factory, cancellationToken: cancellationToken);

        Logs.DebugFinishedOperation(_logger, _className, nameof(GetOrCreateAsync), correlationId, _stopwatch.ElapsedMilliseconds, key);
        return result;
    }

    public async ValueTask CreateAsync<TResult>(Guid correlationId, string key, TResult value, CancellationToken cancellationToken)
    {
        _stopwatch.Restart();

        Logs.DebugStartingOperation(_logger, _className, nameof(CreateAsync), correlationId, key);

        await _cache.SetAsync($"{DefaultConfigurations.ApplicationName}:{key}", value, cancellationToken: cancellationToken);

        Logs.DebugFinishedOperation(_logger, _className, nameof(CreateAsync), correlationId, _stopwatch.ElapsedMilliseconds, key);
    }

    public async ValueTask DeleteAsync(Guid correlationId, string key, CancellationToken cancellationToken)
    {
        _stopwatch.Restart();

        Logs.DebugStartingOperation(_logger, _className, nameof(DeleteAsync), correlationId, key);

        await _cache.RemoveAsync($"{DefaultConfigurations.ApplicationName}:{key}", cancellationToken);

        Logs.DebugFinishedOperation(_logger, _className, nameof(DeleteAsync), correlationId, _stopwatch.ElapsedMilliseconds, key);
    }
}
