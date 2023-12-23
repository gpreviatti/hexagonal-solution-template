using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexagonal.Solution.Template.Data.Tests.Common;

public class TestContainerSqlServerFixture : IDisposable
{
    private readonly MsSqlContainer _sqlServerContainer;
    public string connectionString;
    public MyDbContext gridDbContext;

    public TestContainerSqlServerFixture()
    {
        _sqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(Guid.NewGuid().ToString())
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .Build();

        _sqlServerContainer.StartAsync().Wait();

        RunScripts().Wait();

        connectionString = GetConnectionString();

        var contextOptions = new DbContextOptionsBuilder<MyDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        gridDbContext = new MyDbContext(contextOptions);
    }
    public async Task RunScripts()
    {
        var scriptMigrations = await File.ReadAllTextAsync("./Migrations/CreateTables.sql");
        var scriptSeeds = await File.ReadAllTextAsync("./Seeds/Inserts.sql");

        using var connection = new SqlConnection(GetConnectionString());
        connection.Open();

        using var commandMigrations = new SqlCommand(scriptMigrations, connection);
        await commandMigrations.ExecuteNonQueryAsync();

        using var commandSeeds = new SqlCommand(scriptSeeds, connection);
        await commandSeeds.ExecuteNonQueryAsync();

        connection.Close();
    }

    private string GetConnectionString() =>
        _sqlServerContainer.GetConnectionString().Replace("localhost", "127.0.0.1");

    public void Dispose()
    {
        _sqlServerContainer.StopAsync().Wait();
        gridDbContext.Dispose();
    }
}
