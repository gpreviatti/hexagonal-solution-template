﻿using Application.Common.Messages;
using Application.Common.Repositories;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Application.Common.UseCases;

public abstract class BaseOutUseCase<TRequest, TResponseData, TEntity>(
    IServiceProvider serviceProvider
) where TResponseData : class
  where TEntity : DomainEntity
  where TRequest : class
{
    protected readonly ILogger logger = serviceProvider.GetService<ILogger>();
    protected readonly IBaseRepository<DomainEntity> _repository = serviceProvider.GetRequiredService<IBaseRepository<DomainEntity>>();

    public async Task<BaseResponse<TResponseData>> HandleAsync(CancellationToken cancellationToken)
        => await HandleInternalAsync(cancellationToken);

    public abstract Task<BaseResponse<TResponseData>> HandleInternalAsync(CancellationToken cancellationToken);
}
