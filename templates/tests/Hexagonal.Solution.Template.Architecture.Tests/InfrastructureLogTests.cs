using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Hexagonal.Solution.Template.Infrastructure.Log;

namespace Hexagonal.Solution.Template.Architecture.Tests;
public sealed class InfrastructureLogTests
{
    [Fact(DisplayName = nameof(Infrastructure_Log_Should_Has_Valid_Scopes))]
    public void Infrastructure_Log_Should_Has_Valid_Scopes()
    {
        // Arrange
        ServiceCollection serviceCollection = new();
        var configuration = new ConfigurationBuilder().Build();

        serviceCollection.AddInfrastructureLogServices();

        // Act
        var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

        // Assert
        serviceProvider.Should().NotBeNull();
    }
}
