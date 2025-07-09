using Application.Common.Messages;
using Application.Common.Repositories;
using Domain.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Application.Common.UseCases;

public abstract class BaseInOutUseCase<TRequest, TResponseData, TEntity>(
    IServiceProvider serviceProvider,
    IValidator<TRequest> validator = null
)
    where TRequest : BaseRequest
    where TResponseData : class
    where TEntity : DomainEntity
{
    protected readonly IServiceProvider serviceProvider = serviceProvider;
    protected readonly ILogger logger = serviceProvider.GetRequiredService<ILogger>();
    protected readonly IValidator<TRequest> validator = validator;
    protected readonly IBaseRepository<TEntity> _repository = serviceProvider.GetRequiredService<IBaseRepository<TEntity>>();

    private const string ClassName = nameof(BaseInOutUseCase<TRequest, TResponseData, TEntity>);

    public async Task<BaseResponse<TResponseData>> Handle(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        string methodName = nameof(Handle);
        var response = new BaseResponse<TResponseData>();

        if (validator != null)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(", ", validationResult.Errors);
                response.SetRequestValidationErrorMessage(ClassName, methodName, request.CorrelationId, errors);

                logger.Error("[{ClassName}] | [{methodName}] | [{CorrelationId}] | [{errors}]", ClassName, methodName, request.CorrelationId, errors, response);
                return response;
            }
        }

        return await HandleInternalAsync(request, cancellationToken);
    }

    public abstract Task<BaseResponse<TResponseData>> HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
