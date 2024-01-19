﻿using Hexagonal.Solution.Template.Application.Common.Messages;
using Microsoft.Extensions.DependencyInjection;
using Hexagonal.Solution.Template.Application;

namespace Hexagonal.Solution.Template.Architecture.Tests;
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
        result.IsSuccessful.Should().BeTrue();
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
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact(DisplayName = nameof(Application_Should_Has_Valid_Scopes))]
    public void Application_Should_Has_Valid_Scopes()
    {
        // Arrange
        ServiceCollection serviceCollection = new();

        serviceCollection.AddApplicationServices();

        // Act
        var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

        // Assert
        serviceProvider.Should().NotBeNull();
    }
}
