using Application.Common.Requests;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Services;
using Application.Common.Constants;
using System.Diagnostics.Metrics;
using Application.Common.Repositories;
using Application.Common.Helpers;

namespace Application.Common.UseCases;

public interface IBaseInUseCase<TRequest> where TRequest : BaseRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInUseCase<TRequest> : BaseUseCase, IBaseInUseCase<TRequest> where TRequest : BaseRequest
{
    protected IHybridCacheService Cache { get; }
    protected IProduceService ProduceService { get; }
    protected IBaseRepository Repository { get; }
    private readonly IValidator<TRequest> _validator;
    private readonly Histogram<int> _useCaseExecuted;
    protected const string HandleMethodName = nameof(HandleAsync);

    protected BaseInUseCase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Cache = serviceProvider.GetRequiredService<IHybridCacheService>();
        ProduceService = serviceProvider.GetRequiredService<IProduceService>();
        Repository = serviceProvider.GetRequiredService<IBaseRepository>();
        _validator = serviceProvider.GetRequiredService<IValidator<TRequest>>();

        _useCaseExecuted = DefaultConfigurations.Meter
            .CreateHistogram<int>($"{ClassName}.Executed", "total", "Number of times the use case was executed");
    }

    public async Task HandleAsync(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        Activity = ActivitySource.StartActivity($"{ClassName}.{HandleMethodName}")!;

        Logs.StartingOperation(Logger, ClassName, HandleMethodName, request.CorrelationId);

        if (_validator != null)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                Logs.ValidationErrors(Logger, ClassName, HandleMethodName, request.CorrelationId, errors);

                return;
            }
        }

        await HandleInternalAsync(request, cancellationToken);

        Logs.FinishedOperation(Logger, ClassName, HandleMethodName, request.CorrelationId);

        _useCaseExecuted.Record(1);

        Activity?.SetTag("correlationId", request.CorrelationId);
        Activity?.Stop();
    }

    public abstract Task HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
