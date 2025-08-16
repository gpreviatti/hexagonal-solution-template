using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.Services;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Common.UseCases;

public interface IBaseOutUseCase<TResponseData, TEntity, TUseCase>
    where TResponseData : class
    where TEntity : DomainEntity
{
    Task<BaseResponse<TResponseData>> HandleAsync(CancellationToken cancellationToken);
}

public abstract class BaseOutUseCase<TResponseData, TEntity, TUseCase>(
    IServiceProvider serviceProvider
) : IBaseOutUseCase<TResponseData, TEntity, TUseCase>
    where TResponseData : class
    where TEntity : DomainEntity
    where TUseCase : class
{
    protected readonly ILogger<TUseCase> logger = serviceProvider.GetService<ILogger<TUseCase>>();
    protected readonly IBaseRepository<TEntity> _repository = serviceProvider.GetRequiredService<IBaseRepository<TEntity>>();
    protected readonly IHybridCacheService _cache = serviceProvider.GetRequiredService<IHybridCacheService>();

    public async Task<BaseResponse<TResponseData>> HandleAsync(CancellationToken cancellationToken)
        => await HandleInternalAsync(cancellationToken);

    public abstract Task<BaseResponse<TResponseData>> HandleInternalAsync(CancellationToken cancellationToken);
}
