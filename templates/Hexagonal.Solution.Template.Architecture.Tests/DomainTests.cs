using Hexagonal.Solution.Template.Domain.Common;

namespace Hexagonal.Solution.Template.Architecture.Tests;
public class DomainTests
{
    private static readonly Assembly _domainAssembly = typeof(Result).Assembly;

    [Theory(DisplayName = nameof(Domain_Do_Not_Have_Classes_With_Not_Alloed_Names))]
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
    public void Domain_Do_Not_Have_Classes_With_Not_Alloed_Names(string notAllowedClassName)
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
        result.IsSuccessful.Should().BeTrue();
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
        result.IsSuccessful.Should().BeTrue();
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
        result.IsSuccessful.Should().BeTrue();
    }
}
