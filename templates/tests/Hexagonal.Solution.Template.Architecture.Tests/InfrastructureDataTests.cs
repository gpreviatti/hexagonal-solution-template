using Microsoft.Extensions.DependencyInjection;
using Hexagonal.Solution.Template.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace Hexagonal.Solution.Template.Architecture.Tests;
public sealed class InfrastructureDataTests
{
    [Fact(DisplayName = nameof(Infrastructure_Data_Should_Has_Valid_Scopes))]
    public void Infrastructure_Data_Should_Has_Valid_Scopes()
    {
        // Arrange
        ServiceCollection serviceCollection = new();
        var configuration = new ConfigurationBuilder().Build();

        serviceCollection.AddInfrastructureDataServices(configuration);

        // Act
        var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

        // Assert
        serviceProvider.Should().NotBeNull();
    }
}
