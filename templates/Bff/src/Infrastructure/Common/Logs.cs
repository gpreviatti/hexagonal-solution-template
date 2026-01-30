using System.Net;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Common;

public static partial class Logs
{
    /// <summary>
    /// Logs the sending of a request
    /// </summary>
    /// <param name="logger">logger instance for logging</param>
    /// <param name="className">class name of the calling service</param>
    /// <param name="method">method name of the calling service</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [{Method}] | Sending request"
    )]
    public static partial void SendingRequest(ILogger logger, string className, string method);

    /// <summary>
    /// Logs the sending of a request
    /// </summary>
    /// <param name="logger">logger instance for logging</param>
    /// <param name="className">class name of the calling service</param>
    /// <param name="method">method name of the calling service</param>
    /// <param name="httpMethod">HTTP method of the request</param>
    /// <param name="requestUri">URI of the request</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [{Method}] | [{HttpMethod}] | [{RequestUri}] | Sending request"
    )]
    public static partial void SendingRequest(ILogger logger, string className, string method, HttpMethod httpMethod, string requestUri);

    /// <summary>
    /// Logs a completed request with elapsed time
    /// </summary>
    /// <param name="logger">logger instance for logging</param>
    /// <param name="className">class name of the calling service</param>
    /// <param name="method">method name of the calling service</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [{Method}] | Request completed"
    )]
    public static partial void RequestCompleted(ILogger logger, string className, string method);


    /// <summary>
    /// Logs a completed request with elapsed time
    /// </summary>
    /// <param name="logger">logger instance for logging</param>
    /// <param name="className">class name of the calling service</param>
    /// <param name="method">method name of the calling service</param>
    /// <param name="elapsedMilliseconds">elapsed time in milliseconds</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [{Method}] | Request completed in {ElapsedMilliseconds} ms"
    )]
    public static partial void RequestCompletedWithElapsed(ILogger logger, string className, string method, long elapsedMilliseconds);

    /// <summary>
    /// Logs a completed request with elapsed time
    /// </summary>
    /// <param name="logger">logger instance for logging</param>
    /// <param name="className">class name of the calling service</param>
    /// <param name="method">method name of the calling service</param>
    /// <param name="httpMethod">HTTP method of the request</param>
    /// <param name="requestUri">URI of the request</param>
    /// <param name="elapsedMilliseconds">elapsed time in milliseconds</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [{Method}] | [{HttpMethod}] | [{RequestUri}] | Request completed in {ElapsedMilliseconds} ms"
    )]
    public static partial void RequestCompletedWithElapsed(ILogger logger, string className, string method, HttpMethod httpMethod, string requestUri, long elapsedMilliseconds);

    /// <summary>
    /// Logs a failed request
    /// </summary>
    /// <param name="logger">logger instance for logging</param>
    /// <param name="className">class name of the calling service</param>
    /// <param name="method">method name of the calling service</param>
    /// <param name="message">message associated with the request</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Error,
        Message = "[{ClassName}] | [{Method}] | Request failed | Message: {Message}"
    )]
    public static partial void RequestFailed(ILogger logger, string className, string method, string message);

    /// <summary>
    /// Logs a failed request with elapsed time
    /// </summary>
    /// <param name="logger">logger instance for logging</param>
    /// <param name="className">class name of the calling service</param>
    /// <param name="method">method name of the calling service</param>
    /// <param name="elapsedMilliseconds">elapsed time in milliseconds</param>
    /// <param name="exception">exception that occurred</param>
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Error,
        Message = "[{ClassName}] | [{Method}] | Failed in {ElapsedMilliseconds} ms"
    )]
    public static partial void RequestFailedWithElapsed(ILogger logger, string className, string method, long elapsedMilliseconds, Exception exception);

    /// <summary>
    /// Logs a failed request with elapsed time
    /// </summary>
    /// <param name="logger">logger instance for logging</param>
    /// <param name="className">class name of the calling service</param>
    /// <param name="method">method name of the calling service</param>
    /// <param name="httpMethod">HTTP method of the request</param>
    /// <param name="requestUri">URI of the request</param>
    /// <param name="message">message associated with the request</param>
    /// <param name="statusCode">HTTP status code of the response</param>
    /// <param name="elapsedMilliseconds">elapsed time in milliseconds</param>
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Error,
        Message = "[{ClassName}] | [{Method}] | [{HttpMethod}] | [{RequestUri}] | [{Message}] | {StatusCode} | Request failed with status in {ElapsedMilliseconds} ms"
    )]
    public static partial void RequestFailedWithElapsed(ILogger logger, string className, string method, HttpMethod httpMethod, string requestUri, string? message, HttpStatusCode statusCode, long elapsedMilliseconds);
}