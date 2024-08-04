using AutoFixture;
using Domain.Orders;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace IntegrationTests.Common;
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IDisposable where TProgram : class
{
    protected string? _connectionString = "Server=127.0.0.1,1433;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=true;";

    public MyDbContext? MyDbContext;
    private readonly Fixture? _autoFixture = new();

    public CustomWebApplicationFactory() => SeedDatabase().Wait();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MyDbContext>));

            services.Remove(dbContextDescriptor!);

            var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));

            services.Remove(dbConnectionDescriptor!);

            services.AddDbContext<MyDbContext>((container, options) => options.UseSqlServer(_connectionString));
        });
    }

    public async Task SeedDatabase()
    {
        var contextOptions = new DbContextOptionsBuilder<MyDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        MyDbContext = new(contextOptions);

        if (!MyDbContext.Database.CanConnect())
            throw new InvalidDataException("Unable to connect to database");

        var items = _autoFixture!
            .Build<Item>()
            .Without(i => i.Id)
            .CreateMany(5)
            .ToList();

        var orders = _autoFixture
            .Build<Order>()
            .Without(o => o.Id)
            .With(o => o.Items, items)
            .CreateMany(10);

        await MyDbContext.AddRangeAsync(orders);

        await MyDbContext.SaveChangesAsync();
    }

    public new void Dispose()
    {
        MyDbContext!.Dispose();
        base.Dispose();
    }
}
