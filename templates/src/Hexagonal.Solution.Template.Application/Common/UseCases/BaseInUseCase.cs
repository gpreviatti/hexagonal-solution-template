using FluentValidation;
using Hexagonal.Solution.Template.Application.Common.Messages;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Hexagonal.Solution.Template.Application.Common;

public abstract class BaseInUseCase<TRequest, TResponseData>(
    IServiceProvider serviceProvider,
    IValidator<TRequest> validator = null
)
    where TRequest : class
    where TResponseData : class
{
    protected readonly ILogger logger = serviceProvider.GetService<ILogger>();
    private readonly IValidator<TRequest> validator = validator;

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
