using System.Diagnostics;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Grpc;

public partial class BaseGrpcService<TGrpcService> where TGrpcService : ClientBase<TGrpcService>
{
    /// <summary>
    /// gRPC client instance
    /// </summary>
    protected TGrpcService Client { get; }
    /// <summary>
    /// Logger instance for logging
    /// </summary>
    public ILogger Logger { get; }
    /// <summary>
    /// Stopwatch instance for measuring request duration
    /// </summary>
    public Stopwatch Stopwatch { get; } = new();
    /// <summary>
    /// Class name of the derived gRPC service
    /// </summary>
    public string ClassName { get; }

    /// <summary>
    /// Base gRPC service constructor
    /// </summary>
    /// <param name="logger">logger instance for logging</param>
    /// <param name="grpcClientFactory">gRPC client factory for creating gRPC clients</param>
    public BaseGrpcService(ILogger logger, GrpcClientFactory grpcClientFactory)
    {
        var classType = GetType();
        ClassName = classType.Name;
        Logger = logger;
        Client = grpcClientFactory.CreateClient<TGrpcService>(ClassName);
    }

    /// <summary>
    /// Executes a gRPC handler with logging and error handling
    /// </summary>
    /// <typeparam name="TResponse">TResponse type returned by the gRPC handler</typeparam>
    /// <param name="handler">handler function representing the gRPC call</param>
    /// <returns>Task representing the asynchronous operation, containing the gRPC response</returns>
    protected async Task<TResponse> ExecuteHandlerAsync<TResponse>(Func<AsyncUnaryCall<TResponse>> handler) where TResponse : class
    {
        Stopwatch.Restart();

        try
        {
            StartingRequestLog(Logger, ClassName);

            var response = handler.Invoke();

            RequestCompletedLog(Logger, ClassName, Stopwatch.ElapsedMilliseconds);

            return await response.ResponseAsync;
        }
        catch (RpcException rpcEx)
        {
            RequestFailedLog(Logger, ClassName, Stopwatch.ElapsedMilliseconds, rpcEx);

            throw;
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

