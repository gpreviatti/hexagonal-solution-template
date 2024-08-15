using Application.Common.Messages;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Application.Common.UseCases;

public abstract class BaseInOutUseCase<TRequest, TResponseData>(
    IServiceProvider serviceProvider,
    IValidator<TRequest> validator = null
)
    where TRequest : class
    where TResponseData : class
{
    protected readonly IServiceProvider serviceProvider = serviceProvider;
    protected readonly ILogger logger = serviceProvider.GetService<ILogger>();
    protected readonly IValidator<TRequest> validator = validator;

    public async Task<BaseResponse<TResponseData>> Handle(
        TRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = new BaseResponse<TResponseData>();

        if (validator != null)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                response.SetRequestValidationErrorMessage(errors);

                logger.Error(errors, response);
                return response;
            }
        }

        return await HandleInternalAsync(request, cancellationToken);
    }

    public abstract Task<BaseResponse<TResponseData>> HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}