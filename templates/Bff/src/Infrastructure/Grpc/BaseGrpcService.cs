using System.Diagnostics;
using System.Linq.Expressions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Grpc;

public abstract class BaseGrpcService<TService> where TService : class
{
    protected GrpcChannel Channel { get; }
    protected ILoggerFactory _loggerFactory;
    protected readonly Stopwatch _stopwatch = new();
    protected readonly string _className = typeof(TService).Name;

    public BaseGrpcService(string baseAddress, ILogger<TService> logger)
    {
        Channel = GrpcChannel.ForAddress(baseAddress);
        _loggerFactory = logger;
    }

    public async Task<TResponse> HandleAsync<TRequest, TResponse>(
        Task<Func<TResponse>> action
    ) where TRequest : class where TResponse : class
    {
        _stopwatch.Start();
        try
        {
            _logger.LogInformation("[{ClassName}] | [HandleAsync] | Sending gRPC request", _className);

            var response = await HandleInternalAsync<TRequest, TResponse>();

            _logger.LogInformation(
                "[{ClassName}] | [HandleAsync] | gRPC request completed in {ElapsedMilliseconds} ms",
                _className,
                _stopwatch.ElapsedMilliseconds
            );

            return response;
        }
        catch (RpcException rpcException)
        {
            _logger.LogError(
                rpcException,
                "[{ClassName}] | [HandleAsync] | gRPC request failed with status {Status} in {ElapsedMilliseconds} ms",
                _className,
                rpcException.Status,
                _stopwatch.ElapsedMilliseconds
            );
            throw;
        }
    }

    public virtual Task<TResponse> HandleInternalAsync<TRequest, TResponse>() where TRequest : class where TResponse : class;
}

