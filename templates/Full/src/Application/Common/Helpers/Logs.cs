using Microsoft.Extensions.Logging;

namespace Application.Common.Helpers;

public partial class Logs
{
    /// <summary>
    /// Logs a generic debug message with a custom message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="message">The debug message.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "[{MethodName}] | [{CorrelationId}] | {Message}"
    )]
    public static partial void Debug(ILogger logger, string methodName, Guid correlationId, string message);

    /// <summary>
    /// Logs a generic information message with a custom message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="message">The information message.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "[{MethodName}] | [{CorrelationId}] | {Message}"
    )]
    public static partial void Information(ILogger logger, string methodName, Guid correlationId, string message);

    /// <summary>
    /// Logs a generic warning message with a custom message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="message">The warning message.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "[{MethodName}] | [{CorrelationId}] | {Message}"
    )]
    public static partial void Warning(ILogger logger, string methodName, Guid correlationId, string message);

    /// <summary>
    /// Logs a generic operation failure with a custom message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="message">The failure message.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "[{MethodName}] | Error: {Message}"
    )]
    public static partial void Error(ILogger logger, string methodName, string message);

    /// <summary>
    /// Logs a generic operation failure with a custom message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="message">The failure message.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "[{MethodName}] | [{CorrelationId}] | Error: {Message}"
    )]
    public static partial void Error(ILogger logger, string methodName, Guid correlationId, string message);
    
    /// <summary>
    /// Logs the start of the execution of an operation, including the method name and correlation ID.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="methodName">The name of the method where the operation is executed.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "[{MethodName}] | [{CorrelationId}] | Starting operation"
    )]
    public static partial void StartingOperation(ILogger logger, string methodName, Guid correlationId);

    /// <summary>
    /// Logs the completion of the execution of an operation, including the method name and correlation ID.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="methodName">The name of the method where the operation is executed.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "[{MethodName}] | [{CorrelationId}] | Finished operation"
    )]
    public static partial void FinishedOperation(ILogger logger, string methodName, Guid correlationId);

    /// <summary>
    /// Logs validation errors encountered during request validation.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="methodName">The name of the method where the validation occurred.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="errors">The validation errors.</param>
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Error,
        Message = "[{MethodName}] | [{CorrelationId}] | Validation errors: {Errors}"
    )]
    public static partial void ValidationErrors(ILogger logger, string methodName, Guid correlationId, string errors);

    /// <summary>
    /// Logs when an entity is not found.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="entityName">The name of the entity that was not found.</param>
    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Warning,
        Message = "[{MethodName}] | [{CorrelationId}] | {EntityName} not found."
    )]
    public static partial void NotFound(ILogger logger, string methodName, Guid correlationId, string entityName);

    /// <summary>
    /// Logs a generic operation failure with a custom message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="message">The failure message.</param>
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Warning,
        Message = "[{MethodName}] | [{CorrelationId}] | Failed operation: {Message}"
    )]
    public static partial void FailedOperation(ILogger logger, string methodName, Guid correlationId, string message);

    /// <summary>
    /// Logs the start of an operation.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="method">The method name.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Debug,
        Message = "[{Method}] | [{CorrelationId}] | Starting operation. | {Details}"
    )]
    public static partial void DebugStartingOperation(ILogger logger, string method, Guid correlationId, string details = "");

    /// <summary>
    /// Logs the completion of an operation.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="method">The method name.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Debug,
        Message = "[{Method}] | [{CorrelationId}] | Finished operation. | {Details}"
    )]
    public static partial void DebugFinishedOperation(ILogger logger, string method, Guid correlationId, string details = "");
}