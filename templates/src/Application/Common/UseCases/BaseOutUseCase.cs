﻿using Application.Common.Repositories;
using Application.Common.Requests;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Application.Common.UseCases;

public interface IBaseOutUseCase<TResponseData, TEntity>
    where TResponseData : class
    where TEntity : DomainEntity
{
    Task<BaseResponse<TResponseData>> HandleAsync(CancellationToken cancellationToken);
}

public abstract class BaseOutUseCase<TResponseData, TEntity>(
    IServiceProvider serviceProvider
) : IBaseOutUseCase<TResponseData, TEntity>
    where TResponseData : class
    where TEntity : DomainEntity
{
    protected readonly ILogger logger = serviceProvider.GetService<ILogger>();
    protected readonly IBaseRepository<TEntity> _repository = serviceProvider.GetRequiredService<IBaseRepository<TEntity>>();

    public async Task<BaseResponse<TResponseData>> HandleAsync(CancellationToken cancellationToken)
        => await HandleInternalAsync(cancellationToken);

    public abstract Task<BaseResponse<TResponseData>> HandleInternalAsync(CancellationToken cancellationToken);
}
