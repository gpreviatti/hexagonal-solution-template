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
            StartingRequest(Logger, ClassName);

            var response = await handler(request);

            RequestCompleted(Logger, ClassName, Stopwatch.ElapsedMilliseconds);
            return await response.ResponseAsync;
        }
        catch (Exception ex)
        {
            RequestFailed(Logger, ClassName, Stopwatch.ElapsedMilliseconds, ex);

            throw;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [ExecuteHandlerAsync] | Starting request"
    )]
    public static partial void StartingRequest(ILogger logger, string className);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[{ClassName}] | [ExecuteHandlerAsync] | Completed in {ElapsedMilliseconds} ms"
    )]
    public static partial void RequestCompleted(ILogger logger, string className, long elapsedMilliseconds);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "[{ClassName}] | [ExecuteHandlerAsync] | Failed in {ElapsedMilliseconds} ms"
    )]
    public static partial void RequestFailed(ILogger logger, string className, long elapsedMilliseconds, Exception exception);
}

