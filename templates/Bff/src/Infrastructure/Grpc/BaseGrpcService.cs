using System.Diagnostics;
using System.Runtime.CompilerServices;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using Infrastructure.Common;
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
    /// <param name="methodName">name of the calling method (automatically provided)</param>
    /// <returns>Task representing the asynchronous operation, containing the gRPC response</returns>
    protected async Task<TResponse> ExecuteHandlerAsync<TResponse>(
        Func<AsyncUnaryCall<TResponse>> handler,
        [CallerMemberName] string? methodName = null
    ) where TResponse : class
    {
        Stopwatch.Restart();

        try
        {
           Logs.SendingRequest(Logger, ClassName, methodName!);

            var response = handler.Invoke();

            Logs.RequestCompletedWithElapsed(Logger, ClassName, methodName!, Stopwatch.ElapsedMilliseconds);
            return await response.ResponseAsync;
        }
        catch (RpcException rpcEx)
        {
            Logs.RequestFailedWithElapsed(Logger, ClassName, methodName!, Stopwatch.ElapsedMilliseconds, rpcEx);

            throw;
        }
        catch (Exception ex)
        {
            Logs.RequestFailedWithElapsed(Logger, ClassName, methodName!, Stopwatch.ElapsedMilliseconds, ex);

            throw;
        }
    }
}

