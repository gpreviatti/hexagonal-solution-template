using Application.Common.Requests;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Services;
using Application.Common.Repositories;
using Application.Common.Helpers;

namespace Application.Common.UseCases;

public interface IBaseInUseCase<TRequest> where TRequest : BaseRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInUseCase<TRequest>(IServiceProvider serviceProvider) : BaseUseCase(serviceProvider), IBaseInUseCase<TRequest> where TRequest : BaseRequest
{
    protected IHybridCacheService Cache { get; } = serviceProvider.GetRequiredService<IHybridCacheService>();
    protected IProduceService ProduceService { get; } = serviceProvider.GetRequiredService<IProduceService>();
    protected IBaseRepository Repository { get; } = serviceProvider.GetRequiredService<IBaseRepository>();
    private readonly IValidator<TRequest> _validator = serviceProvider.GetRequiredService<IValidator<TRequest>>();
    protected const string HandleMethodName = nameof(HandleAsync);

    public async Task HandleAsync(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        using var activity = ActivitySource.StartActivity($"{ClassName}");

        Logs.StartingOperation(Logger, request.CorrelationId);

        if (_validator != null)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                Logs.ValidationErrors(Logger, request.CorrelationId, errors);
                UseCaseFailedMetric.Add(1);
                return;
            }
        }

        await HandleInternalAsync(request, cancellationToken);

        Logs.FinishedOperation(Logger, request.CorrelationId);

        UseCaseExecutedMetric.Add(1);
    }

    public abstract Task HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
