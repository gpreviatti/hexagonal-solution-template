using Application.Common.Repositories;
using CommonTests.Fixtures;
using Domain.Common;
using IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using WebApp;

namespace IntegrationTests.Data;

public class BaseDataFixture<TEntity> : BaseFixture where TEntity : DomainEntity
{
    public required IBaseRepository<TEntity> repository;

    public void SetRepository(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<TEntity>>();
    }
}