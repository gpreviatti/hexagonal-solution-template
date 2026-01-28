using System.Diagnostics;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Grpc;

public partial class BaseGrpcService
{
    protected GrpcChannel Channel { get; }
    public ILogger Logger { get; }
    public Stopwatch Stopwatch { get; } = new();
    public string ClassName { get; }

    public BaseGrpcService(string baseAddress, ILogger logger)
    {
        var classType = GetType();
        ClassName = classType.Name;
        Channel = GrpcChannel.ForAddress(baseAddress);
        Logger = logger;
    }

    protected async Task<TResponse> ExecuteHandlerAsync<TRequest, TResponse>(
        TRequest request,
        Func<TRequest, Task<AsyncUnaryCall<TResponse>>> handler
    )
        where TRequest : class
        where TResponse : class
    {
        Stopwatch.Restart();
        try
        {
            StartingRequestLog(Logger, ClassName);

            var response = await handler(request);

            RequestCompletedLog(Logger, ClassName, Stopwatch.ElapsedMilliseconds);
            return await response.ResponseAsync;
        }
        catch (Exception ex)
        {
            RequestFailedLog(Logger, ClassName, Stopwatch.ElapsedMilliseconds, ex);

            throw;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [ExecuteHandlerAsync] | Starting request"
    )]
    public static partial void StartingRequestLog(ILogger logger, string className);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [ExecuteHandlerAsync] | Completed in {ElapsedMilliseconds} ms"
    )]
    public static partial void RequestCompletedLog(ILogger logger, string className, long elapsedMilliseconds);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "[{ClassName}] | [ExecuteHandlerAsync] | Failed in {ElapsedMilliseconds} ms"
    )]
    public static partial void RequestFailedLog(ILogger logger, string className, long elapsedMilliseconds, Exception exception);
}

