using Application.Common.Messages;
using Application.Common.Repositories;
using Domain.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Application.Common.UseCases;

public abstract class BaseInUseCase<TRequest, TResponseData, TEntity>(
    IServiceProvider serviceProvider,
    IValidator<TRequest> validator = null
)
    where TRequest : class
    where TResponseData : class
    where TEntity : DomainEntity
{
    protected readonly ILogger logger = serviceProvider.GetService<ILogger>();
    protected readonly IValidator<TRequest> validator = validator;
    protected readonly IBaseRepository<DomainEntity> _repository = serviceProvider.GetRequiredService<IBaseRepository<DomainEntity>>();

    public async Task HandleAsync(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
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

    public abstract Task<BaseResponse<TResponseData>> HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
