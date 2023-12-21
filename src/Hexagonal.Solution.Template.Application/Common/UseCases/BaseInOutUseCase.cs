using FluentValidation;
using Hexagonal.Solution.Template.Application.Common.Messages;
using Serilog;

namespace Hexagonal.Solution.Template.Application.Common;

public abstract class BaseInOutUseCase<TRequest, TResponseData>(
    ILogger logger,
    IValidator<TRequest>? validator = null
)
    where TRequest : class
    where TResponseData : class
{
    protected readonly IValidator<TRequest>? _validator = validator;
    protected readonly ILogger logger = logger;

    public async Task<BaseResponse<TResponseData>> Handle(
        TRequest request, 
        CancellationToken cancellationToken
    )
    {
        var response = new BaseResponse<TResponseData>();

        if (_validator != null)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                response.Success = false;
                response.Message = errors;

                logger.Error(errors, response);
                return response;
            }
        }

        return await HandleInternalAsync(request, cancellationToken);
    }

    public abstract Task<BaseResponse<TResponseData>> HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
