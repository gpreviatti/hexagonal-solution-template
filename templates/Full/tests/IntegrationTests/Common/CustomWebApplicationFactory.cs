using System.Data.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApp;

namespace IntegrationTests.Common;

[CollectionDefinition("WebApplicationFactoryCollectionDefinition")]
public sealed class WebApplicationFactoryCollectionDefinition : IClassFixture<CustomWebApplicationFactory<Program>>;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IDisposable where TProgram : class
{
    protected string? _connectionString = "Server=127.0.0.1,1433;Database=OrderDb;User Id=sa;Password=cY5VvZkkh4AzES;TrustServerCertificate=true;";

    public MyDbContext? MyDbContext;

    public CustomWebApplicationFactory() => SetDbContext();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "IntegrationTests");

        builder.ConfigureTestServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MyDbContext>));

            services.Remove(dbContextDescriptor!);

            var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));

            services.Remove(dbConnectionDescriptor!);

            services.AddDbContextFactory<MyDbContext>((options) => options.UseSqlServer(_connectionString));
        });
    }

    public void SetDbContext()
    {
        var contextOptions = new DbContextOptionsBuilder<MyDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        MyDbContext = new(contextOptions);

        if (!MyDbContext.Database.CanConnect())
            throw new InvalidDataException("Unable to connect to database");
    }

    public new void Dispose()
    {
        MyDbContext!.Dispose();
        // base.Dispose();
    }
}
