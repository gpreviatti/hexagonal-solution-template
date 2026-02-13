using System.Reflection;
using Domain;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using NetArchTest.Rules;

namespace UnitTests.Architecture;
public sealed class DomainTests
{
    private static readonly Assembly _domainAssembly = typeof(Result).Assembly;

    [Theory(DisplayName = nameof(DomainDoNotHaveClassesWithNotAllowedNames))]
    [InlineData("Dto")]
    [InlineData("Dtos")]
    [InlineData("UseCase")]
    [InlineData("UseCases")]
    [InlineData("Request")]
    [InlineData("Response")]
    [InlineData("Message")]
    [InlineData("Validation")]
    [InlineData("Controller")]
    [InlineData("Repository")]
    [InlineData("Query")]
    public void DomainDoNotHaveClassesWithNotAllowedNames(string notAllowedClassName)
    {
        // Arrange, Act
        var result = Types
            .InAssembly(_domainAssembly)
            .That()
            .AreClasses()
            .Should()
            .NotHaveNameEndingWith(notAllowedClassName)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = nameof(DomainDoNotHaveApplicationDependency))]
    public void DomainDoNotHaveApplicationDependency()
    {
        // Arrange, Act
        var result = Types
            .InAssembly(_domainAssembly)
            .Should()
            .NotHaveDependencyOn("Application")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = nameof(DomainDoNotHaveInfrastructureDependency))]
    public void DomainDoNotHaveInfrastructureDependency()
    {
        // Arrange, Act
        var result = Types
            .InAssembly(_domainAssembly)
            .Should()
            .NotHaveDependencyOn("Infrastructure")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = nameof(DomainShouldHasValidScopes))]
    public void DomainShouldHasValidScopes()
    {
        // Arrange
        ServiceCollection serviceCollection = new();

        serviceCollection.AddDomain();

        // Act
        var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

        // Assert
        Assert.NotNull(serviceProvider);
    }
}
