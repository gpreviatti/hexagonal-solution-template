using Application.Common.Requests;
using Application.Common.Repositories;
using Domain.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Common.UseCases;

public interface IBaseInUseCase<TRequest, TEntity, TUseCase>
    where TRequest : BaseRequest
    where TEntity : DomainEntity
    where TUseCase : class
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInUseCase<TRequest, TEntity, TUseCase>(
    IServiceProvider serviceProvider,
    IValidator<TRequest> validator = null
) : IBaseInUseCase<TRequest, TEntity, TUseCase>
    where TRequest : BaseRequest
    where TEntity : DomainEntity
    where TUseCase : class
{
    protected readonly ILogger<TUseCase> logger = serviceProvider.GetService<ILogger<TUseCase>>();
    protected readonly IValidator<TRequest> validator = validator;
    protected readonly IBaseRepository<TEntity> _repository = serviceProvider.GetRequiredService<IBaseRepository<TEntity>>();
    private const string ClassName = nameof(BaseInUseCase<TRequest, TEntity, TUseCase>);

    public async Task HandleAsync(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        string methodName = nameof(HandleAsync);
        logger.LogInformation("[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Start to execute use case", ClassName, methodName, request.CorrelationId);

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
    }

    public abstract Task HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
