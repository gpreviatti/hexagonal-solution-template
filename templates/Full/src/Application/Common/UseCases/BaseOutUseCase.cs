using Application.Common.Requests;
using Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Helpers;

namespace Application.Common.UseCases;

public interface IBaseOutUseCase<TResponseData> where TResponseData : BaseResponse
{
    Task<TResponseData> HandleAsync(CancellationToken cancellationToken);
}

public abstract class BaseOutUseCase<TResponseData>(IServiceProvider serviceProvider) : BaseUseCase(serviceProvider), IBaseOutUseCase<TResponseData> where TResponseData : BaseResponse
{
    protected IHybridCacheService Cache { get; } = serviceProvider.GetRequiredService<IHybridCacheService>();
    protected IProduceService ProduceService { get; } = serviceProvider.GetRequiredService<IProduceService>();
    protected const string HandleMethodName = nameof(HandleAsync);

    public async Task<TResponseData> HandleAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity($"{ClassName}");
                
        var correlationId = Guid.NewGuid();
        Logs.StartingOperation(Logger, correlationId);

        var response = await HandleInternalAsync(cancellationToken);

        Logs.FinishedOperation(Logger, correlationId);

        UseCaseExecutedMetric.Add(1);

        return response;
    }

    public abstract Task<TResponseData> HandleInternalAsync(CancellationToken cancellationToken);
}
