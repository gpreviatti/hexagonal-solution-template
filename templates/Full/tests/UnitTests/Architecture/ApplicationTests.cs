using System.Reflection;
using Application;
using Application.Common.Requests;
using Microsoft.Extensions.DependencyInjection;
using NetArchTest.Rules;

namespace UnitTests.Architecture;
public sealed class ApplicationTests
{
    private static readonly Assembly _applicationAssembly = typeof(BaseResponse).Assembly;

    [Theory(DisplayName = nameof(ApplicationDoNotHaveClassesWithNotAllowedNames))]
    [InlineData("Entity")]
    [InlineData("ValueObject")]
    [InlineData("Vo")]
    [InlineData("Service")]
    [InlineData("Controller")]
    public void ApplicationDoNotHaveClassesWithNotAllowedNames(string notAllowedClassName)
    {
        // Arrange, Act
        var result = Types
            .InAssembly(_applicationAssembly)
            .That()
            .AreClasses()
            .Should()
            .NotHaveNameEndingWith(notAllowedClassName)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = nameof(ApplicationDoNotHaveInfrastructureDependency))]
    public void ApplicationDoNotHaveInfrastructureDependency()
    {
        // Arrange, Act
        var result = Types
            .InAssembly(_applicationAssembly)
            .Should()
            .NotHaveDependencyOn("Infrastructure")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = nameof(ApplicationShouldHasValidScopes))]
    public void ApplicationShouldHasValidScopes()
    {
        // Arrange
        ServiceCollection serviceCollection = new();

        serviceCollection.AddApplication();

        // Act
        var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

        // Assert
        Assert.NotNull(serviceProvider);
    }
}
