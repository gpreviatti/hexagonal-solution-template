using Application.Common.Repositories;
using CommonTests.Fixtures;
using IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using WebApp;

namespace IntegrationTests.Data;

public class BaseDataFixture : BaseFixture
{
    public required IBaseRepository Repository { get; set; }

    public void SetRepository(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        Repository = scope.ServiceProvider.GetRequiredService<IBaseRepository>();
    }
}
