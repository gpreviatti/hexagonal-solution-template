﻿using System.Diagnostics.CodeAnalysis;
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

        builder.UseSqlServer("Server=127.0.0.1,1433;Database=OrderDb;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=true;");

        return new MyDbContext(builder.Options);
    }
}
