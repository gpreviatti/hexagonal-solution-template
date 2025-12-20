using Application.Common.Requests;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Application.Common.Services;
using Application.Common.Constants;
using System.Diagnostics.Metrics;
using Application.Common.Repositories;

namespace Application.Common.UseCases;

public interface IBaseInUseCase<TRequest> where TRequest : BaseRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInUseCase<TRequest> : BaseUseCase, IBaseInUseCase<TRequest> where TRequest : BaseRequest
{
    protected readonly IHybridCacheService _cache;
    protected readonly IProduceService _produceService;
    protected readonly IBaseRepository _repository;
    private readonly IValidator<TRequest> _validator;
    private readonly Histogram<int> _useCaseExecuted;
    private readonly Gauge<long> _useCaseExecutionElapsedTime;
    protected const string HandleMethodName = nameof(HandleAsync);

    protected BaseInUseCase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _cache = serviceProvider.GetRequiredService<IHybridCacheService>();
        _produceService = serviceProvider.GetRequiredService<IProduceService>();
        _repository = serviceProvider.GetRequiredService<IBaseRepository>();
        _validator = serviceProvider.GetRequiredService<IValidator<TRequest>>();

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
        stopWatch.Restart();

        logger.LogInformation(
            DefaultApplicationMessages.StartToExecuteUseCase,
            ClassName, HandleMethodName, request.CorrelationId
        );

        if (_validator != null)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                logger.LogError(
                    DefaultApplicationMessages.ValidationErrors,
                    ClassName, HandleMethodName, request.CorrelationId, errors
                );

                return;
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
