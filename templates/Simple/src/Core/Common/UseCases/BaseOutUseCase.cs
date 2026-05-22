using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Core.Common.Helpers;
using Core.Common.Requests;
using Core.Common.Services;
using Core.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Common.UseCases;

public interface IBaseOutUseCase<TResponseData> where TResponseData : BaseResponse
{
    Task<TResponseData> HandleAsync(CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage]
public abstract class BaseOutUseCase<TResponseData>(IServiceProvider serviceProvider) : BaseUseCase(serviceProvider), IBaseOutUseCase<TResponseData> where TResponseData : BaseResponse
{
    protected IHybridCacheService Cache { get; } = serviceProvider.GetRequiredService<IHybridCacheService>();
    protected const string HandleMethodName = nameof(HandleAsync);

    public async Task<TResponseData> HandleAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity($"{ClassName}");
        activity.SetDefaultTags();

        var correlationId = Guid.NewGuid();
        Logs.StartingOperation(Logger, correlationId);

        var response = await HandleInternalAsync(cancellationToken);

        Logs.FinishedOperation(Logger, correlationId);

        UseCaseExecutedMetric.Add(1);

        activity?.SetStatus(response.Success ? ActivityStatusCode.Ok : ActivityStatusCode.Error, response.Message);

        return response;
    }

    public abstract Task<TResponseData> HandleInternalAsync(CancellationToken cancellationToken);
}
