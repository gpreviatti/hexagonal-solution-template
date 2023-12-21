using Hexagonal.Solution.Template.Application.Common.Messages;
using Serilog;

namespace Hexagonal.Solution.Template.Application.Common;

public abstract class BaseOutUseCase<TRequest, TResponseData>(
    ILogger logger
)
    where TResponseData : class
{
    private readonly ILogger logger = logger;

    public async Task<BaseResponse<TResponseData>> HandleAsync(CancellationToken cancellationToken)
    {
        return await HandleInternalAsync(cancellationToken);
    }

    public abstract Task<BaseResponse<TResponseData>> HandleInternalAsync(CancellationToken cancellationToken);
}
