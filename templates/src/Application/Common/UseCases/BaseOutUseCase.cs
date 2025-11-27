using System.Diagnostics;
using System.Diagnostics.Metrics;
using Application.Common.Constants;
using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Common.UseCases;

public interface IBaseOutUseCase<TResponseData> where TResponseData : BaseResponse
{
    Task<TResponseData> HandleAsync(CancellationToken cancellationToken);
}

public abstract class BaseOutUseCase<TResponseData>: IBaseOutUseCase<TResponseData> where TResponseData : BaseResponse
{
    protected readonly IServiceProvider serviceProvider;
    protected readonly ILogger logger;
    protected readonly IBaseRepository _repository;
    protected readonly IHybridCacheService _cache;
    protected readonly IProduceService _produceService;
    protected string ClassName;
    private readonly Histogram<int> _useCaseExecuted;
    private readonly Gauge<long> _useCaseExecutionElapsedTime;
    protected readonly Stopwatch _stopWatch = new();
    protected const string HandleMethodName = nameof(HandleAsync);

    protected BaseOutUseCase(IServiceProvider serviceProvider)
    {
        var classType = GetType();
        ClassName = classType.Name;

        this.serviceProvider = serviceProvider;
        logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(classType);
        _repository = serviceProvider.GetRequiredService<IBaseRepository>();
        _cache = serviceProvider.GetRequiredService<IHybridCacheService>();
        _produceService = serviceProvider.GetRequiredService<IProduceService>();

        _useCaseExecuted = DefaultConfigurations.Meter
            .CreateHistogram<int>($"{ClassName}.Executed", "total", "Number of times the use case was executed");

        _useCaseExecutionElapsedTime = DefaultConfigurations.Meter
            .CreateGauge<long>($"{ClassName}.Elapsed", "elapsed", "Elapsed time taken to execute the use case");
    }

    public async Task<TResponseData> HandleAsync(CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid();
        logger.LogInformation(DefaultApplicationMessages.StartToExecuteUseCase, ClassName, HandleMethodName, correlationId);

        var response = await HandleInternalAsync(cancellationToken);

        logger.LogInformation(
            DefaultApplicationMessages.FinishedExecutingUseCase,
            ClassName, HandleMethodName, correlationId, stopWatch.ElapsedMilliseconds
        );

        _useCaseExecuted.Record(1);
        _useCaseExecutionElapsedTime.Record(stopWatch.ElapsedMilliseconds);

        return response;
    }

    public abstract Task<TResponseData> HandleInternalAsync(CancellationToken cancellationToken);
}
