using Application.Common.Requests;
using Application.Common.Repositories;
using Domain.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Application.Common.Constants;

namespace Application.Common.UseCases;

public interface IBaseInOutUseCase<TRequest, TResponseData, TUseCase>
    where TRequest : BaseRequest
    where TResponseData : class
    where TUseCase : class
{
    Task<BaseResponse<TResponseData>> Handle(TRequest request, CancellationToken cancellationToken);
}

public abstract class BaseInOutUseCase<TRequest, TResponseData, TEntity, TUseCase>(
    IServiceProvider serviceProvider,
    IValidator<TRequest> validator = null
) : IBaseInOutUseCase<TRequest, TResponseData, TUseCase>
    where TRequest : BaseRequest
    where TResponseData : class
    where TEntity : DomainEntity
    where TUseCase : class
{
    protected readonly IServiceProvider serviceProvider = serviceProvider;
    protected readonly ILogger<TUseCase> logger = serviceProvider.GetRequiredService<ILogger<TUseCase>>();
    protected readonly IValidator<TRequest> validator = validator;
    protected readonly IBaseRepository<TEntity> _repository = serviceProvider.GetRequiredService<IBaseRepository<TEntity>>();

    private const string ClassName = nameof(BaseInOutUseCase<TRequest, TResponseData, TEntity, TUseCase>);

    public async Task<BaseResponse<TResponseData>> Handle(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        string methodName = nameof(Handle);
        logger.LogInformation(DefaultApplicationMessages.StartToExecuteUseCase, ClassName, methodName, request.CorrelationId);

        var response = new BaseResponse<TResponseData>();

        if (validator != null)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(", ", validationResult.Errors);
                response.SetMessage(errors);

                logger.LogError(DefaultApplicationMessages.ValidationErrors, ClassName, methodName, request.CorrelationId, errors, response);
                return response;
            }
        }

        return await HandleInternalAsync(request, cancellationToken);
    }

    public abstract Task<BaseResponse<TResponseData>> HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
