using System.Reflection;
using Domain;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using NetArchTest.Rules;

namespace UnitTests.Architecture;
public sealed class DomainTests
{
    private static readonly Assembly _domainAssembly = typeof(Result).Assembly;

    [Theory(DisplayName = nameof(Domain_Do_Not_Have_Classes_With_Not_Allowed_Names))]
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
    public void Domain_Do_Not_Have_Classes_With_Not_Allowed_Names(string notAllowedClassName)
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

    [Fact(DisplayName = nameof(Domain_Do_Not_Have_Application_Dependency))]
    public void Domain_Do_Not_Have_Application_Dependency()
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

    [Fact(DisplayName = nameof(Domain_Do_Not_Have_Infrastructure_Dependency))]
    public void Domain_Do_Not_Have_Infrastructure_Dependency()
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

    [Fact(DisplayName = nameof(Domain_Should_Has_Valid_Scopes))]
    public void Domain_Should_Has_Valid_Scopes()
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
