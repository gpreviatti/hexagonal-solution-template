using Application.Common.Requests;
using Application.Common.Repositories;
using Domain.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Application.Common.UseCases;

public interface IBaseInUseCase<TRequest, TEntity>
    where TRequest : BaseRequest
    where TEntity : DomainEntity
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInUseCase<TRequest, TEntity>(
    IServiceProvider serviceProvider,
    IValidator<TRequest> validator = null
) : IBaseInUseCase<TRequest, TEntity>
    where TRequest : BaseRequest
    where TEntity : DomainEntity
{
    protected readonly ILogger logger = serviceProvider.GetService<ILogger>();
    protected readonly IValidator<TRequest> validator = validator;
    protected readonly IBaseRepository<TEntity> _repository = serviceProvider.GetRequiredService<IBaseRepository<TEntity>>();
    private const string ClassName = nameof(BaseInUseCase<TRequest, TEntity>);

    public async Task HandleAsync(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        string methodName = nameof(HandleAsync);
        logger.Information("[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Start to execute use case", ClassName, methodName, request.CorrelationId);

        if (validator != null)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                logger.Error(errors);
            }
        }

        await HandleInternalAsync(request, cancellationToken);
    }

    public abstract Task HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
