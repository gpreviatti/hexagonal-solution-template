using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Common;

public partial class Logs
{
    /// <summary>
    /// Logs a generic debug message with a custom message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="message">The debug message.</param>
    /// <param name="methodName">The name of the method (auto-captured).</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "[{MethodName}] | {Message}"
    )]
    public static partial void Debug(ILogger logger, string message, [CallerMemberName] string methodName = null!);

    /// <summary>
    /// Logs a generic information message with a custom message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="message">The information message.</param>
    /// <param name="methodName">The name of the method (auto-captured).</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "[{MethodName}] | {Message}"
    )]
    public static partial void Information(ILogger logger, string message, [CallerMemberName] string methodName = null!);

    /// <summary>
    /// Logs a generic warning message with a custom message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="message">The warning message.</param>
    /// <param name="methodName">The name of the method (auto-captured).</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "[{MethodName}] | {Message}"
    )]
    public static partial void Warning(ILogger logger, string message, [CallerMemberName] string methodName = null!);

    /// <summary>
    /// Logs a generic operation failure with a custom message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="message">The failure message.</param>
    /// <param name="methodName">The name of the method (auto-captured).</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "[{MethodName}] | Error: {Message}"
    )]
    public static partial void Error(ILogger logger, string message, [CallerMemberName] string methodName = null!);
    
    /// <summary>
    /// Logs the start of the execution of an operation, including the method name and correlation ID.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="methodName">The name of the method where the operation is executed (auto-captured).</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "[{MethodName}] | Starting operation"
    )]
    public static partial void StartingOperation(ILogger logger, [CallerMemberName] string methodName = null!);

    /// <summary>
    /// Logs the completion of the execution of an operation, including the method name and correlation ID.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="methodName">The name of the method where the operation is executed (auto-captured).</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "[{MethodName}] | Finished operation"
    )]
    public static partial void FinishedOperation(ILogger logger, [CallerMemberName] string methodName = null!);

    /// <summary>
    /// Logs a generic operation failure with a custom message.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="message">The failure message.</param>
    /// <param name="methodName">The name of the method (auto-captured).</param>
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Warning,
        Message = "[{MethodName}] | Failed operation: {Message}"
    )]
    public static partial void FailedOperation(ILogger logger, string message, [CallerMemberName] string methodName = null!);

    /// <summary>
    /// Logs the start of an operation.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="details">Optional details about the operation.</param>
    /// <param name="method">The method name (auto-captured).</param>
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Debug,
        Message = "[{Method}] | Starting operation. | {Details}"
    )]
    public static partial void DebugStartingOperation(ILogger logger, string details = "", [CallerMemberName] string method = null!);

    /// <summary>
    /// Logs the completion of an operation.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="details">Optional details about the operation.</param>
    /// <param name="method">The method name (auto-captured).</param>
    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Debug,
        Message = "[{Method}] | Finished operation. | {Details}"
    )]
    public static partial void DebugFinishedOperation(ILogger logger, string details = "", [CallerMemberName] string method = null!);
}