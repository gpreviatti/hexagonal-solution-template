using System.Diagnostics;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Grpc;

public abstract class BaseGrpcService
{
    protected GrpcChannel Channel { get; }
    protected readonly ILogger logger;
    protected readonly Stopwatch stopwatch = new();
    protected readonly string className;

    public BaseGrpcService(string baseAddress, ILogger logger)
    {
        var classType = GetType();
        className = classType.Name;
        Channel = GrpcChannel.ForAddress(baseAddress);
        this.logger = logger;
    }

    public async Task<TResponse> HandleAsync<TRequest, TResponse>(TRequest request)
        where TRequest : class
        where TResponse : class
    {
        stopwatch.Start();
        try
        {
            logger.LogInformation("[{ClassName}] | [HandleAsync] | Sending gRPC request", className);

            var response = await HandleInternalAsync<TRequest, TResponse>(request);

            logger.LogInformation(
                "[{ClassName}] | [HandleAsync] | gRPC request completed in {ElapsedMilliseconds} ms",
                className,
                stopwatch.ElapsedMilliseconds
            );

            return response;
        }
        catch (RpcException rpcException)
        {
            logger.LogError(
                rpcException,
                "[{ClassName}] | [HandleAsync] | gRPC request failed with status {Status} in {ElapsedMilliseconds} ms",
                className,
                rpcException.Status,
                stopwatch.ElapsedMilliseconds
            );
            throw;
        }
    }

    public abstract Task<TResponse> HandleInternalAsync<TRequest, TResponse>(TRequest request) where TRequest : class where TResponse : class;
}

