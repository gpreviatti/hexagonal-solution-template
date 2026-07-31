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

        builder.UseNpgsql();

        return new(builder.Options);
    }
}
