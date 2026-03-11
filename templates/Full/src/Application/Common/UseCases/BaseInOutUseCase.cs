using System.Diagnostics.Metrics;
using Application.Common.Constants;
using Application.Common.Helpers;
using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.UseCases;

public interface IBaseInOutUseCase<in TRequest, TResponseData>
    where TRequest : BaseRequest
    where TResponseData : BaseResponse
{
    Task<TResponseData> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInOutUseCase<TRequest, TResponseData> : BaseUseCase, IBaseInOutUseCase<TRequest, TResponseData>
    where TRequest : BaseRequest
    where TResponseData : BaseResponse
{
    protected IHybridCacheService Cache { get; }
    protected IProduceService ProduceService { get; }
    protected IBaseRepository Repository { get; }
    private readonly IValidator<TRequest> _validator;
    private readonly Histogram<int> _useCaseExecuted;
    protected const string HandleMethodName = nameof(HandleAsync);

    protected BaseInOutUseCase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Cache = serviceProvider.GetRequiredService<IHybridCacheService>();
        ProduceService = serviceProvider.GetRequiredService<IProduceService>();
        Repository = serviceProvider.GetRequiredService<IBaseRepository>();
        _validator = serviceProvider.GetRequiredService<IValidator<TRequest>>();

        _useCaseExecuted = DefaultConfigurations.Meter
            .CreateHistogram<int>($"{ClassName}.Executed", "total", "Number of times the use case was executed");
    }

    public async Task<TResponseData> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        using var activity = ActivitySource.StartActivity($"{ClassName}.{HandleMethodName}")!;
        
        Logs.StartingOperation(Logger, ClassName, HandleMethodName, request.CorrelationId);
        TResponseData response;

        if (_validator != null)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(", ", validationResult.Errors);
                Logs.ValidationErrors(Logger, ClassName, HandleMethodName, request.CorrelationId, errors);

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

        Logs.FinishedOperation(Logger, ClassName, HandleMethodName, request.CorrelationId);

        _useCaseExecuted.Record(1);
        activity?.SetTag("correlationId", request.CorrelationId);

        return response;
    }

    public abstract Task<TResponseData> HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
