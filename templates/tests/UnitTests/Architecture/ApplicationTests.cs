﻿using System.Reflection;
using Application;
using Application.Common.Messages;
using Microsoft.Extensions.DependencyInjection;
using NetArchTest.Rules;

namespace UnitTests.Architecture;
public sealed class ApplicationTests
{
    private static readonly Assembly _applicationAssembly = typeof(BaseResponse).Assembly;

    [Theory(DisplayName = nameof(Application_Do_Not_Have_Classes_With_Not_Allowed_Names))]
    [InlineData("Entity")]
    [InlineData("ValueObject")]
    [InlineData("Vo")]
    [InlineData("Service")]
    [InlineData("Controller")]
    public void Application_Do_Not_Have_Classes_With_Not_Allowed_Names(string notAllowedClassName)
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

    [Fact(DisplayName = nameof(Application_Do_Not_Have_Infrastructure_Dependency))]
    public void Application_Do_Not_Have_Infrastructure_Dependency()
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

    [Fact(DisplayName = nameof(Application_Should_Has_Valid_Scopes))]
    public void Application_Should_Has_Valid_Scopes()
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
