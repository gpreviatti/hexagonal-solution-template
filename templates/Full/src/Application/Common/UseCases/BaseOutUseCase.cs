using System.Diagnostics.Metrics;
using Application.Common.Constants;
using Application.Common.Requests;
using Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Helpers;

namespace Application.Common.UseCases;

public interface IBaseOutUseCase<TResponseData> where TResponseData : BaseResponse
{
    Task<TResponseData> HandleAsync(CancellationToken cancellationToken);
}

public abstract class BaseOutUseCase<TResponseData> : BaseUseCase, IBaseOutUseCase<TResponseData> where TResponseData : BaseResponse
{
    protected IHybridCacheService Cache { get; }
    protected IProduceService ProduceService { get; }
    private readonly Histogram<int> _useCaseExecuted;
    protected const string HandleMethodName = nameof(HandleAsync);

    protected BaseOutUseCase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Cache = serviceProvider.GetRequiredService<IHybridCacheService>();
        ProduceService = serviceProvider.GetRequiredService<IProduceService>();

        _useCaseExecuted = DefaultConfigurations.Meter
            .CreateHistogram<int>($"{ClassName}.Executed", "total", "Number of times the use case was executed");
    }

    public async Task<TResponseData> HandleAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity();
                
        var correlationId = Guid.NewGuid();
        Logs.StartingOperation(Logger, correlationId);

        var response = await HandleInternalAsync(cancellationToken);

        Logs.FinishedOperation(Logger, correlationId);

        _useCaseExecuted.Record(1);

        return response;
    }

    public abstract Task<TResponseData> HandleInternalAsync(CancellationToken cancellationToken);
}
