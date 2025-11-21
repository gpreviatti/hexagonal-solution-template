using System.Diagnostics;
using System.Diagnostics.Metrics;
using Application.Common.Constants;
using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.Services;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Common.UseCases;

public interface IBaseOutUseCase<TResponseData, TEntity, TUseCase>
    where TResponseData : BaseResponse
    where TEntity : DomainEntity
{
    Task<TResponseData> HandleAsync(CancellationToken cancellationToken);
}

public abstract class BaseOutUseCase<TResponseData, TEntity, TUseCase>(
    IServiceProvider serviceProvider
) : IBaseOutUseCase<TResponseData, TEntity, TUseCase>
    where TResponseData : BaseResponse
    where TEntity : DomainEntity
    where TUseCase : class
{
    protected readonly ILogger<TUseCase> logger = serviceProvider.GetRequiredService<ILogger<TUseCase>>();
    protected readonly IBaseRepository _repository = serviceProvider.GetRequiredService<IBaseRepository>();
    protected readonly IHybridCacheService _cache = serviceProvider.GetRequiredService<IHybridCacheService>();
    private const string ClassName = nameof(BaseOutUseCase<TResponseData, TEntity, TUseCase>);
    private const string HandleMethodName = nameof(HandleAsync);

    private readonly Histogram<int> _useCaseExecuted = DefaultConfigurations.Meter
        .CreateHistogram<int>($"{typeof(TUseCase).Name.ToLower()}.executed", "total", "Number of times the use case was executed");
    private readonly Gauge<long> _useCaseExecutionElapsedTime = DefaultConfigurations.Meter
        .CreateGauge<long>($"{typeof(TUseCase).Name.ToLower()}.elapsed", "milliseconds", "Elapsed time taken to execute the use case");

    public async Task<TResponseData> HandleAsync(CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid();
        logger.LogInformation(DefaultApplicationMessages.StartToExecuteUseCase, ClassName, HandleMethodName, correlationId);

        var response = await HandleInternalAsync(cancellationToken);

        logger.LogInformation(
            DefaultApplicationMessages.FinishedExecutingUseCase,
            ClassName, HandleMethodName, correlationId, stopWatch.ElapsedMilliseconds
        );

        _useCaseExecuted.Record(1);
        _useCaseExecutionElapsedTime.Record(stopWatch.ElapsedMilliseconds);

        return response;
    }

    public abstract Task<TResponseData> HandleInternalAsync(CancellationToken cancellationToken);
}
