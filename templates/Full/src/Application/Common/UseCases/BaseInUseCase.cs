using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Application.Common.Helpers;
using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.Services;
using Domain.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.UseCases;

public interface IBaseInUseCase<in TRequest> where TRequest : BaseRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInUseCase<TRequest>(IServiceProvider serviceProvider) : BaseUseCase(serviceProvider), IBaseInUseCase<TRequest> where TRequest : BaseRequest
{
    protected IHybridCacheService Cache { get; } = serviceProvider.GetRequiredService<IHybridCacheService>();
    protected IBaseRepository Repository { get; } = serviceProvider.GetRequiredService<IBaseRepository>();
    protected const string HandleMethodName = nameof(HandleAsync);

    public async Task HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity($"{ClassName}");
        activity.SetDefaultTags();

        Logs.StartingOperation(Logger, request.CorrelationId);

        var validationContext = new ValidationContext(request);
        if (!Validator.TryValidateObject(request, validationContext, null, true))
        {
            string errors = string.Join(", ", validationContext.Items.Values.SelectMany(v => v as IEnumerable<ValidationResult> ?? []).Select(e => e.ErrorMessage));
            Logs.ValidationErrors(Logger, request.CorrelationId, errors);
            UseCaseFailedMetric.Add(1);
            activity?.SetStatus(ActivityStatusCode.Error, errors);
            return;
        }

        await HandleInternalAsync(request, cancellationToken);

        Logs.FinishedOperation(Logger, request.CorrelationId);

        UseCaseExecutedMetric.Add(1);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    public abstract Task HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
