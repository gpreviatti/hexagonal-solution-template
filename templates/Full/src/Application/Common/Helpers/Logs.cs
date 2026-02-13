using Microsoft.Extensions.Logging;

namespace Application.Common.Helpers;

public partial class Logs
{
    // Use Case Execution Logs
    
    /// <summary>
    /// Logs the start of the execution of a use case, including the class name, method name, and correlation ID.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class where the use case is executed.</param>
    /// <param name="methodName">The name of the method where the use case is executed.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Start to execute use case"
    )]
    public static partial void StartToExecuteUseCase(ILogger logger, string className, string methodName, Guid correlationId);

    /// <summary>
    /// Logs the completion of the execution of a use case, including the class name, method name, correlation ID, and elapsed time in milliseconds.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class where the use case is executed.</param>
    /// <param name="methodName">The name of the method where the use case is executed.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds for the execution of the use case.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Elapsed time: {ElapsedMilliseconds} ms | Finished executing use case"
    )]
    public static partial void FinishedExecutingUseCase(ILogger logger, string className, string methodName, Guid correlationId, long elapsedMilliseconds);

    // Validation Logs

    /// <summary>
    /// Logs validation errors encountered during request validation.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class where the validation occurred.</param>
    /// <param name="methodName">The name of the method where the validation occurred.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="errors">The validation errors.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Validation errors: {Errors}"
    )]
    public static partial void ValidationErrors(ILogger logger, string className, string methodName, Guid correlationId, string errors);

    /// <summary>
    /// Logs when an entity is not found.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="entityName">The name of the entity that was not found.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Warning,
        Message = "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | {EntityName} not found."
    )]
    public static partial void NotFound(ILogger logger, string className, string methodName, Guid correlationId, string entityName);

    /// <summary>
    /// Logs a generic operation failure with a custom message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="message">The failure message.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Warning,
        Message = "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Operation failed: {Message}"
    )]
    public static partial void OperationFailed(ILogger logger, string className, string methodName, Guid correlationId, string message);

    /// <summary>
    /// Logs when starting to publish a message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="messageType">The type of message being published.</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [HandleAsync] | CorrelationId: {CorrelationId} | Publishing message: {MessageType}"
    )]
    public static partial void PublishingMessage(ILogger logger, string className, Guid correlationId, string messageType);

    /// <summary>
    /// Logs when a message has been published successfully.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="messageType">The type of message published.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds.</param>
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [HandleAsync] | CorrelationId: {CorrelationId} | Message published: {MessageType} | Elapsed time: {ElapsedMilliseconds} ms"
    )]
    public static partial void MessagePublished(ILogger logger, string className, Guid correlationId, string messageType, long elapsedMilliseconds);

    /// <summary>
    /// Logs when starting to publish a batch of messages.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="correlationIds">The correlation IDs for tracking the requests.</param>
    /// <param name="messageType">The type of messages being published.</param>
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [HandleAsync] | CorrelationId: {CorrelationIds} | Publishing batch of messages: {MessageType}"
    )]
    public static partial void PublishingBatchMessages(ILogger logger, string className, object correlationIds, string messageType);

    /// <summary>
    /// Logs when a batch of messages has been published successfully.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="correlationIds">The correlation IDs for tracking the requests.</param>
    /// <param name="messageType">The type of messages published.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds.</param>
    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [HandleAsync] | CorrelationId: {CorrelationIds} | Batch of messages published: {MessageType} | Elapsed time: {ElapsedMilliseconds} ms"
    )]
    public static partial void BatchMessagesPublished(ILogger logger, string className, object correlationIds, string messageType, long elapsedMilliseconds);

    /// <summary>
    /// Logs when a message is received by a consumer.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="messageType">The type of message received.</param>
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Received message: {MessageType}"
    )]
    public static partial void ReceivedMessage(ILogger logger, string className, Guid correlationId, string messageType);

    /// <summary>
    /// Logs when a duplicate message is detected.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Warning,
        Message = "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Duplicate message detected. Skipping processing."
    )]
    public static partial void DuplicateMessageDetected(ILogger logger, string className, Guid correlationId);

    /// <summary>
    /// Logs when starting to process a message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Start processing message."
    )]
    public static partial void StartProcessingMessage(ILogger logger, string className, Guid correlationId);

    /// <summary>
    /// Logs when a message has been processed successfully.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds.</param>
    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Processed message in {ElapsedMilliseconds} ms"
    )]
    public static partial void ProcessedMessage(ILogger logger, string className, Guid correlationId, long elapsedMilliseconds);

    /// <summary>
    /// Logs when an error occurs while processing a message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="stackTrace">The stack trace.</param>
    [LoggerMessage(
        EventId = 14,
        Level = LogLevel.Error,
        Message = "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Error processing message: {ErrorMessage} | StackTrace: {StackTrace}"
    )]
    public static partial void ErrorProcessingMessage(ILogger logger, string className, Guid? correlationId, string errorMessage, string? stackTrace);

    /// <summary>
    /// Logs when a null message is received.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="messageType">The type of message expected.</param>
    [LoggerMessage(
        EventId = 15,
        Level = LogLevel.Warning,
        Message = "[{ClassName}] | [HandleMessageAsync] | Received null message of type {MessageType}"
    )]
    public static partial void ReceivedNullMessage(ILogger logger, string className, string messageType);

    /// <summary>
    /// Logs when an error occurs while deserializing a message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="appId">The application ID.</param>
    /// <param name="clusterId">The cluster ID.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="stackTrace">The stack trace.</param>
    [LoggerMessage(
        EventId = 16,
        Level = LogLevel.Error,
        Message = "[{ClassName}] | [HandleRabbitMqAsync] | [{CorrelationId}] | AppId: {AppId} | ClusterId: {ClusterId} | Error deserializing message: {ErrorMessage} | StackTrace: {StackTrace}"
    )]
    public static partial void ErrorDeserializingMessage(ILogger logger, string className, string? correlationId, string? appId, string? clusterId, string errorMessage, string? stackTrace);

    /// <summary>
    /// Logs when an unexpected error occurs.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="appId">The application ID.</param>
    /// <param name="clusterId">The cluster ID.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="stackTrace">The stack trace.</param>
    [LoggerMessage(
        EventId = 17,
        Level = LogLevel.Error,
        Message = "[{ClassName}] | [HandleRabbitMqAsync] | [{CorrelationId}] | AppId: {AppId} | ClusterId: {ClusterId} | Unexpected error: {ErrorMessage} | StackTrace: {StackTrace}"
    )]
    public static partial void UnexpectedError(ILogger logger, string className, string? correlationId, string? appId, string? clusterId, string errorMessage, string? stackTrace);

    /// <summary>
    /// Logs when an exception is handled by the exception handling middleware.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="message">The exception message.</param>
    [LoggerMessage(
        EventId = 18,
        Level = LogLevel.Error,
        Message = "[{ClassName}] | [{MethodName}] | {Message}"
    )]
    public static partial void ExceptionHandlerError(ILogger logger, Exception exception, string className, string methodName, string message);
}