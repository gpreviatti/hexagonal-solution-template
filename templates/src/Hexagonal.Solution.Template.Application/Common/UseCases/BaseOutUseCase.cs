using Hexagonal.Solution.Template.Application.Common.Messages;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Hexagonal.Solution.Template.Application.Common.UseCases;

public abstract class BaseOutUseCase<TRequest, TResponseData>(
    IServiceProvider serviceProvider
) where TResponseData : class
{
    protected readonly ILogger logger = serviceProvider.GetService<ILogger>();

    public async Task<BaseResponse<TResponseData>> HandleAsync(CancellationToken cancellationToken)
        => await HandleInternalAsync(cancellationToken);

    public abstract Task<BaseResponse<TResponseData>> HandleInternalAsync(CancellationToken cancellationToken);
}
