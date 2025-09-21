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
    Task<TResponseData> HandleAsync(CancellationToken cancellationToken, string cacheKey = null);
}

public abstract class BaseOutUseCase<TResponseData, TEntity, TUseCase>(
    IServiceProvider serviceProvider
) : IBaseOutUseCase<TResponseData, TEntity, TUseCase>
    where TResponseData : BaseResponse
    where TEntity : DomainEntity
    where TUseCase : class
{
    protected readonly ILogger<TUseCase> logger = serviceProvider.GetService<ILogger<TUseCase>>();
    protected readonly IBaseRepository<TEntity> _repository = serviceProvider.GetRequiredService<IBaseRepository<TEntity>>();
    protected readonly IHybridCacheService _cache = serviceProvider.GetRequiredService<IHybridCacheService>();
    private const string ClassName = nameof(BaseOutUseCase<TResponseData, TEntity, TUseCase>);
    private const string HandleMethodName = nameof(HandleAsync);

    public async Task<TResponseData> HandleAsync(CancellationToken cancellationToken, string cacheKey = null)
    {
        var correlationId = Guid.NewGuid();
        logger.LogInformation(DefaultApplicationMessages.StartToExecuteUseCase, ClassName, HandleMethodName, correlationId);

        TResponseData response;
        var handleInternalTask = HandleInternalAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            response = await handleInternalTask;

            logger.LogInformation(
                DefaultApplicationMessages.FinishedExecutingUseCase,
                ClassName, HandleMethodName, correlationId
            );
        }
        else
        {
            response = await _cache.GetOrCreateAsync(
                cacheKey,
                async cancellationToken => await handleInternalTask,
                cancellationToken
            );

            logger.LogInformation(
                DefaultApplicationMessages.FinishedExecutingUseCaseWithCache,
                ClassName, HandleMethodName, correlationId, cacheKey
            );
        }

        return response;
    }

    public abstract Task<TResponseData> HandleInternalAsync(CancellationToken cancellationToken);
}
