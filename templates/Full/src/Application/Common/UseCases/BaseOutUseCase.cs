using System.Diagnostics.Metrics;
using Application.Common.Constants;
using Application.Common.Requests;
using Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Application.Common.Helpers;

namespace Application.Common.UseCases;

public interface IBaseOutUseCase<TResponseData> where TResponseData : BaseResponse
{
    Task<TResponseData> HandleAsync(CancellationToken cancellationToken);
}

public abstract class BaseOutUseCase<TResponseData> : BaseUseCase, IBaseOutUseCase<TResponseData> where TResponseData : BaseResponse
{
    protected IHybridCacheService Cache { get; }
    protected IProduceService ProduceService { get; }
    private readonly Histogram<int> _useCaseExecuted;
    private readonly Gauge<long> _useCaseExecutionElapsedTime;
    protected const string HandleMethodName = nameof(HandleAsync);

    protected BaseOutUseCase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Cache = serviceProvider.GetRequiredService<IHybridCacheService>();
        ProduceService = serviceProvider.GetRequiredService<IProduceService>();

        _useCaseExecuted = DefaultConfigurations.Meter
            .CreateHistogram<int>($"{ClassName}.Executed", "total", "Number of times the use case was executed");

        _useCaseExecutionElapsedTime = DefaultConfigurations.Meter
            .CreateGauge<long>($"{ClassName}.Elapsed", "elapsed", "Elapsed time taken to execute the use case");
    }

    public async Task<TResponseData> HandleAsync(CancellationToken cancellationToken)
    {
        StopWatch.Restart();
        var correlationId = Guid.NewGuid();
        Logs.StartToExecuteUseCase(Logger, ClassName, HandleMethodName, correlationId);

        var response = await HandleInternalAsync(cancellationToken);

        Logs.FinishedExecutingUseCase(Logger, ClassName, HandleMethodName, correlationId, StopWatch.ElapsedMilliseconds);

        _useCaseExecuted.Record(1);
        _useCaseExecutionElapsedTime.Record(StopWatch.ElapsedMilliseconds);

        return response;
    }

    public abstract Task<TResponseData> HandleInternalAsync(CancellationToken cancellationToken);
}
