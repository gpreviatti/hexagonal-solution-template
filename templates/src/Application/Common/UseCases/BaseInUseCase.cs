using Application.Common.Requests;
using Application.Common.Repositories;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Application.Common.Services;
using Application.Common.Constants;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Application.Common.UseCases;

public interface IBaseInUseCase<TRequest>where TRequest : BaseRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInUseCase<TRequest> : IBaseInUseCase<TRequest> where TRequest : BaseRequest
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

    protected BaseInUseCase(IServiceProvider serviceProvider)
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

    public async Task HandleAsync(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        var stopWatch = Stopwatch.StartNew();
        logger.LogInformation(
            DefaultApplicationMessages.StartToExecuteUseCase,
            ClassName, HandleMethodName, request.CorrelationId
        );

        var validator = serviceProvider.GetRequiredService<IValidator<TRequest>>();
        if (validator != null)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                logger.LogError(errors);
            }
        }

        await HandleInternalAsync(request, cancellationToken);

        logger.LogInformation(
            DefaultApplicationMessages.FinishedExecutingUseCase,
            ClassName, HandleMethodName, request.CorrelationId, stopWatch.ElapsedMilliseconds
        );

        _useCaseExecuted.Record(1);
        _useCaseExecutionElapsedTime.Record(stopWatch.ElapsedMilliseconds);
    }

    public abstract Task HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
