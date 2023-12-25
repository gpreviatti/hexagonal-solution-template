using FluentValidation;
using Hexagonal.Solution.Template.Application.Common.Messages;
using Serilog;

namespace Hexagonal.Solution.Template.Application.Common;

public abstract class BaseInUseCase<TRequest, TResponseData>(
    ILogger logger,
    IValidator<TRequest> validator = null
)
    where TRequest : class
    where TResponseData : class
{
    private readonly IValidator<TRequest> validator = validator;
    protected readonly ILogger logger = logger;

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
        logger.Information("Handler executed with success.");
    }

    public abstract Task<BaseResponse<TResponseData>> HandleInternalAsync(TRequest request, CancellationToken cancellationToken);
}
