using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

/// <summary>
/// This class is used to generate migrations.
/// Change the database connection string to your local db connection to generate migrations
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ContextFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<MyDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__OrderDb") ??
            "Host=127.0.0.1;Port=5432;Database=OrderDb;Username=postgres;Password=cY5VvZkkh4AzES";

        builder.UseNpgsql(connectionString);

        return new MyDbContext(builder.Options);
    }
}
