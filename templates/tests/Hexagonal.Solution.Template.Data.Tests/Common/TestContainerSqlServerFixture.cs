﻿using Hexagonal.Solution.Template.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Hexagonal.Solution.Template.Data.Tests.Common;

public class TestContainerSqlServerFixture : IDisposable
{
    private readonly MsSqlContainer _sqlServerContainer;
    public string connectionString;
    public MyDbContext myDbContext;

    public TestContainerSqlServerFixture()
    {
        _sqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(Guid.NewGuid().ToString())
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .Build();

        _sqlServerContainer.StartAsync().Wait();

        connectionString = GetConnectionString();

        var contextOptions = new DbContextOptionsBuilder<MyDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        myDbContext = new MyDbContext(contextOptions);
    }

    private string GetConnectionString() =>
        _sqlServerContainer.GetConnectionString().Replace("localhost", "127.0.0.1");

    public void Dispose()
    {
        _sqlServerContainer.StopAsync().Wait();
        myDbContext.Dispose();
    }
}
