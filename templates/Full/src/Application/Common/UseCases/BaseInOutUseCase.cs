using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Application.Common.Helpers;
using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.Services;
using Domain.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.UseCases;

public interface IBaseInOutUseCase<in TRequest, TResponseData>
    where TRequest : BaseRequest
    where TResponseData : BaseResponse
{
    Task<TResponseData> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInOutUseCase<TRequest, TResponseData>(IServiceProvider serviceProvider) : BaseUseCase(serviceProvider), IBaseInOutUseCase<TRequest, TResponseData>
    where TRequest : BaseRequest
    where TResponseData : BaseResponse
{
    protected IHybridCacheService Cache { get; } = serviceProvider.GetRequiredService<IHybridCacheService>();
    protected IBaseRepository Repository { get; } = serviceProvider.GetRequiredService<IBaseRepository>();
    protected const string HandleMethodName = nameof(HandleAsync);

    public async Task<TResponseData> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        using var activity = ActivitySource.StartActivity($"{ClassName}");
        activity.SetDefaultTags();
        activity?.SetTag("correlationId", request.CorrelationId);

        Logs.StartingOperation(Logger, request.CorrelationId);
        TResponseData response;

        var validationContext = new ValidationContext(request);
        if (!Validator.TryValidateObject(request, validationContext, null, true))
        {
            string errors = string.Join(", ", validationContext.Items.Values.SelectMany(v => v as IEnumerable<ValidationResult> ?? []).Select(e => e.ErrorMessage));

            Logs.ValidationErrors(Logger, request.CorrelationId, errors);

            response = Activator.CreateInstance<TResponseData>();
            response = response with
            {
                Success = false,
                Message = errors
            };

            UseCaseFailedMetric.Add(1);

            activity?.SetStatus(ActivityStatusCode.Error, errors);

            return response!;
        }

        response = await HandleInternalAsync(request, cancellationToken);

        Logs.FinishedOperation(Logger, request.CorrelationId);

        UseCaseExecutedMetric.Add(1);

        activity?.SetStatus(response.Success ? ActivityStatusCode.Ok : ActivityStatusCode.Error, response.Message);

        return response;
    }

    public abstract Task<TResponseData> HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
