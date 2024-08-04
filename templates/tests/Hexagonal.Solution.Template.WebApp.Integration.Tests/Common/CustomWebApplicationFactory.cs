using AutoFixture;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using Testcontainers.MsSql;

namespace Hexagonal.Solution.Template.WebApp.Integration.Tests.Common;
public sealed class CustomWebApplicationFactory<TProgram> :  WebApplicationFactory<TProgram>, IDisposable where TProgram : class
{
    private MsSqlContainer _sqlServerContainer;
    protected string _connectionString;

    public MyDbContext myDbContext;
    private readonly Fixture autoFixture = new();

    public CustomWebApplicationFactory()
    {
        ConfigureTestSqlServerContainer();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MyDbContext>));

            services.Remove(dbContextDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));

            services.Remove(dbConnectionDescriptor);

            services.AddDbContext<MyDbContext>((container, options) =>
            {
                options.UseSqlServer(_connectionString);
            });
        });
    }

    public void ConfigureTestSqlServerContainer()
    {
        _sqlServerContainer = new MsSqlBuilder()
            .WithName("sql-server-2022-webapp-integration-tests")
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPortBinding(1434, 1433)
            .WithPassword(Guid.NewGuid().ToString())
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .Build();

        _sqlServerContainer.StartAsync().Wait();

        _connectionString = _sqlServerContainer.GetConnectionString().Replace("localhost", "127.0.0.1");

        MigrateAndSeedDatabase().Wait();
    }

    public async Task MigrateAndSeedDatabase()
    {
        var contextOptions = new DbContextOptionsBuilder<MyDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        myDbContext = new(contextOptions);

        myDbContext.Database.Migrate();

        var items = autoFixture
            .Build<Item>()
            .Without(i => i.Id)
            .CreateMany(5)
            .ToList();

        var orders = autoFixture
            .Build<Order>()
        .Without(o => o.Id)
            .With(o => o.Items, items)
        .CreateMany(10);

        await myDbContext.AddRangeAsync(orders);

        await myDbContext.SaveChangesAsync();
    }   

    public void Dispose()
    {
        _sqlServerContainer.StopAsync().Wait();
        myDbContext.Dispose();
        base.Dispose();
    }
}
