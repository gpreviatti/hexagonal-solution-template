using Microsoft.Extensions.Logging;

namespace Infrastructure.Common;

internal static partial class Logs
{
    /// <summary>
    /// Logs an unexpected error in a background service.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="stackTrace">The stack trace of the error.</param>
    [LoggerMessage(
        EventId = 200,
        Level = LogLevel.Error,
        Message = "[BaseBackgroundService] | [ExecuteAsync] | Unexpected error in background service. | Message: {ErrorMessage} | StackTrace: {StackTrace}"
    )]
    public static partial void UnexpectedErrorInBackgroundService(ILogger logger, string errorMessage, string? stackTrace);

    /// <summary>
    /// Logs the start of a database operation.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="method">The method name.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    [LoggerMessage(
        EventId = 201,
        Level = LogLevel.Debug,
        Message = "[BaseRepository] | [{Method}] | CorrelationId: {CorrelationId} | Starting database operation."
    )]
    public static partial void StartingDatabaseOperation(ILogger logger, string method, Guid correlationId);

    /// <summary>
    /// Logs the completion of a database query.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="method">The method name.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds.</param>
    [LoggerMessage(
        EventId = 202,
        Level = LogLevel.Debug,
        Message = "[BaseRepository] | [{Method}] | CorrelationId: {CorrelationId} | Query executed in {ElapsedMilliseconds} ms."
    )]
    public static partial void QueryExecuted(ILogger logger, string method, Guid correlationId, long elapsedMilliseconds);

    /// <summary>
    /// Logs the start of retrieving a cache entry.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="key">The cache key.</param>
    [LoggerMessage(
        EventId = 203,
        Level = LogLevel.Debug,
        Message = "[HybridCacheService] | [GetOrCreateAsync] | [{Key}] | Retrieving cache entry"
    )]
    public static partial void RetrievingCacheEntry(ILogger logger, string key);

    /// <summary>
    /// Logs the completion of retrieving a cache entry.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="key">The cache key.</param>
    [LoggerMessage(
        EventId = 204,
        Level = LogLevel.Debug,
        Message = "[HybridCacheService] | [GetOrCreateAsync] | [{Key}] | Cache entry retrieved"
    )]
    public static partial void CacheEntryRetrieved(ILogger logger, string key);

    /// <summary>
    /// Logs the start of creating a cache entry.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="key">The cache key.</param>
    [LoggerMessage(
        EventId = 205,
        Level = LogLevel.Debug,
        Message = "[HybridCacheService] | [CreateAsync] | [{Key}] | Creating cache entry"
    )]
    public static partial void CreatingCacheEntry(ILogger logger, string key);

    /// <summary>
    /// Logs the completion of creating a cache entry.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="key">The cache key.</param>
    [LoggerMessage(
        EventId = 206,
        Level = LogLevel.Debug,
        Message = "[HybridCacheService] | [CreateAsync] | [{Key}] | Cache entry created"
    )]
    public static partial void CacheEntryCreated(ILogger logger, string key);

    /// <summary>
    /// Logs the start of deleting a cache entry.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="key">The cache key.</param>
    [LoggerMessage(
        EventId = 207,
        Level = LogLevel.Debug,
        Message = "[HybridCacheService] | [DeleteAsync] | [{Key}] | Deleting cache entry"
    )]
    public static partial void DeletingCacheEntry(ILogger logger, string key);

    /// <summary>
    /// Logs the completion of deleting a cache entry.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="key">The cache key.</param>
    [LoggerMessage(
        EventId = 208,
        Level = LogLevel.Debug,
        Message = "[HybridCacheService] | [DeleteAsync] | [{Key}] | Cache entry deleted"
    )]
    public static partial void CacheEntryDeleted(ILogger logger, string key);
}
