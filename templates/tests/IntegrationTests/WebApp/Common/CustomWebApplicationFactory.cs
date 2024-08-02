using AutoFixture;
using Domain.Orders;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace IntegrationTests.WebApp.Common;
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IDisposable where TProgram : class
{
    protected string? _connectionString = "Server=127.0.0.1,1433;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=true;";

    public MyDbContext? myDbContext;
    private readonly Fixture? autoFixture = new();

    public CustomWebApplicationFactory() => MigrateAndSeedDatabase().Wait();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MyDbContext>));

            services.Remove(dbContextDescriptor!);

            var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));

            services.Remove(dbConnectionDescriptor!);

            services.AddDbContext<MyDbContext>((container, options) =>
            {
                options.UseSqlServer(_connectionString);
            });
        });
    }

    public async Task MigrateAndSeedDatabase()
    {
        var contextOptions = new DbContextOptionsBuilder<MyDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        myDbContext = new(contextOptions);

        myDbContext.Database.Migrate();

        var items = autoFixture!
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

    public new void Dispose()
    {
        myDbContext!.Dispose();
        base.Dispose();
    }
}
