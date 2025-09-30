using Application.Common.Requests;
using Application.Common.Repositories;
using Domain.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Application.Common.Constants;
using Application.Common.Services;
using System.Diagnostics;

namespace Application.Common.UseCases;

public interface IBaseInOutUseCase<TRequest, TResponseData, TUseCase>
    where TRequest : BaseRequest
    where TResponseData : BaseResponse
    where TUseCase : class
{
    Task<TResponseData> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInOutUseCase<TRequest, TResponseData, TEntity, TUseCase>(
    IServiceProvider serviceProvider,
    IValidator<TRequest> validator = null
) : IBaseInOutUseCase<TRequest, TResponseData, TUseCase>
    where TRequest : BaseRequest
    where TResponseData : BaseResponse
    where TEntity : DomainEntity
    where TUseCase : class
{
    protected readonly IServiceProvider serviceProvider = serviceProvider;
    protected readonly ILogger<TUseCase> logger = serviceProvider.GetRequiredService<ILogger<TUseCase>>();
    protected readonly IValidator<TRequest> validator = validator;
    protected readonly IBaseRepository<TEntity> _repository = serviceProvider.GetRequiredService<IBaseRepository<TEntity>>();
    protected readonly IHybridCacheService _cache = serviceProvider.GetRequiredService<IHybridCacheService>();
    protected string ClassName = typeof(TUseCase).Name;
    protected const string HandleMethodName = nameof(HandleAsync);

    public async Task<TResponseData> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        var stopWatch = Stopwatch.StartNew();

        logger.LogInformation(
            DefaultApplicationMessages.StartToExecuteUseCase,
            ClassName, HandleMethodName, request.CorrelationId
        );
        TResponseData response;

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
                response.Success = false;
                response.Message = errors;

                return response!;
            }
        }

        response = await HandleInternalAsync(request, cancellationToken);

        logger.LogInformation(
            DefaultApplicationMessages.FinishedExecutingUseCase,
            ClassName, HandleMethodName, request.CorrelationId, stopWatch.ElapsedMilliseconds
        );

        return response;
    }

    public abstract Task<TResponseData> HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
