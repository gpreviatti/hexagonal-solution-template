using System.Diagnostics;
using System.Diagnostics.Metrics;
using Application.Common.Constants;
using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Common.UseCases;

public interface IBaseInOutUseCase<in TRequest, TResponseData>
    where TRequest : BaseRequest
    where TResponseData : BaseResponse
{
    Task<TResponseData> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInOutUseCase<TRequest, TResponseData> : IBaseInOutUseCase<TRequest, TResponseData>
    where TRequest : BaseRequest
    where TResponseData : BaseResponse
{
    protected readonly IServiceProvider serviceProvider;
    protected readonly ILogger logger;
    protected readonly IBaseRepository _repository;
    protected readonly IHybridCacheService _cache;
    protected readonly IProduceService _produceService;
    protected readonly Stopwatch _stopWatch = new();
    protected string ClassName;
    protected const string HandleMethodName = nameof(HandleAsync);
    private readonly Histogram<int> _useCaseExecuted;
    private readonly Gauge<long> _useCaseExecutionElapsedTime;

    protected BaseInOutUseCase(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;

        logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
        _repository = serviceProvider.GetRequiredService<IBaseRepository>();
        _cache = serviceProvider.GetRequiredService<IHybridCacheService>();
        _produceService = serviceProvider.GetRequiredService<IProduceService>();

        ClassName = GetType().Name;

        _useCaseExecuted = DefaultConfigurations.Meter
            .CreateHistogram<int>($"{ClassName.ToLower()}.executed", "total", "Number of times the use case was executed");

        _useCaseExecutionElapsedTime = DefaultConfigurations.Meter
            .CreateGauge<long>($"{ClassName.ToLower()}.elapsed", "elapsed", "Elapsed time taken to execute the use case");
    }

    public async Task<TResponseData> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        _stopWatch.Restart();

        logger.LogInformation(
            DefaultApplicationMessages.StartToExecuteUseCase,
            ClassName, HandleMethodName, request.CorrelationId
        );
        TResponseData response;

        var validator = serviceProvider.GetRequiredService<IValidator<TRequest>>();
        if (validator != null)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(", ", validationResult.Errors);
                logger.LogError(
                    DefaultApplicationMessages.ValidationErrors,
                    ClassName, HandleMethodName, request.CorrelationId, errors
                );

                response = Activator.CreateInstance<TResponseData>();
                response = response with
                {
                    Success = false,
                    Message = errors
                };

                return response!;
            }
        }

        response = await HandleInternalAsync(request, cancellationToken);

        logger.LogInformation(
            DefaultApplicationMessages.FinishedExecutingUseCase,
            ClassName, HandleMethodName, request.CorrelationId, _stopWatch.ElapsedMilliseconds
        );

        _useCaseExecuted.Record(1);
        _useCaseExecutionElapsedTime.Record(_stopWatch.ElapsedMilliseconds);

        return response;
    }

    public abstract Task<TResponseData> HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
