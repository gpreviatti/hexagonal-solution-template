using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Hexagonal.Solution.Template.Architecture.Tests;
public sealed class FunctionAppTests
{
    [Fact(DisplayName = nameof(FunctionApp_Should_Not_Have_Missing_Dependencies))]
    public void FunctionApp_Should_Not_Have_Missing_Dependencies()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        var configuration = new ConfigurationBuilder().Build();

        Host.FunctionApp.Program.AddDependencies(serviceCollection, configuration);

        // Act
        var serviceProvider = serviceCollection.BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        // Assert
        serviceProvider.Should().NotBeNull();

    }
}
