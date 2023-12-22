using Hexagonal.Solution.Template.Application.Common.Messages;

namespace Hexagonal.Solution.Template.Architecture.Tests;
public class ApplicationTests
{
    private static readonly Assembly _applicationAssembly = typeof(BaseResponse).Assembly;

    [Theory(DisplayName = nameof(Application_Do_Not_Have_Classes_With_Not_Alloed_Names))]
    [InlineData("Entity")]
    [InlineData("ValueObject")]
    [InlineData("Vo")]
    [InlineData("Service")]
    [InlineData("Controller")]
    public void Application_Do_Not_Have_Classes_With_Not_Alloed_Names(string notAllowedClassName)
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
}
