using Application.Common.Requests;
using Application.Common.Repositories;
using Domain.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Application.Common.Services;
using Application.Common.Constants;
using System.Diagnostics;

namespace Application.Common.UseCases;

public interface IBaseInUseCase<in TRequest, TEntity, TUseCase>
    where TRequest : BaseRequest
    where TEntity : DomainEntity
    where TUseCase : class
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInUseCase<TRequest, TEntity, TUseCase>(
    IServiceProvider serviceProvider,
    IValidator<TRequest> validator = null!
) : IBaseInUseCase<TRequest, TEntity, TUseCase>
    where TRequest : BaseRequest
    where TEntity : DomainEntity
    where TUseCase : class
{
    protected readonly ILogger<TUseCase> logger = serviceProvider.GetRequiredService<ILogger<TUseCase>>();
    protected readonly IValidator<TRequest> validator = validator;
    protected readonly IBaseRepository _repository = serviceProvider.GetRequiredService<IBaseRepository>();
    protected readonly IHybridCacheService _cache = serviceProvider.GetRequiredService<IHybridCacheService>();
    private const string ClassName = nameof(BaseInUseCase<TRequest, TEntity, TUseCase>);
    private const string HandleMethodName = nameof(HandleAsync);

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
    }

    public abstract Task HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
