using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Core.Common.Helpers;
using Core.Common.Repositories;
using Core.Common.Requests;
using Core.Common.Services;
using Core.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Common.UseCases;

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

    public async Task<TResponseData> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity($"{ClassName}");
        activity.SetDefaultTags();
        activity?.SetTag("correlationId", request.CorrelationId);

        Logs.StartingOperation(Logger, request.CorrelationId);
        TResponseData response;

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new(request), validationResults, true))
        {
            string errors = string.Join(", ", validationResults.Select(e => e.ErrorMessage));

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
