using WebUi;

namespace UnitTests.Architecture;

public sealed class WebUiTests
{
    private static readonly System.Reflection.Assembly _assembly = typeof(Program).Assembly;

    [Theory(DisplayName = nameof(GivenTheWebUiAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheEntryPointNaming))]
    [InlineData("Entity")]
    [InlineData("ValueObject")]
    [InlineData("Vo")]
    [InlineData("Repository")]
    [InlineData("DbContext")]
    [InlineData("Mapping")]
    [InlineData("Consumer")]
    [InlineData("Producer")]
    public void GivenTheWebUiAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheEntryPointNaming(string forbiddenSuffix) => ArchitectureTestHelper.AssertClassNamesDoNotEndWith(_assembly, forbiddenSuffix);

    [Fact(DisplayName = nameof(GivenTheWebUiProjectWhenCheckingProjectReferencesThenItShouldOnlyReferenceTheInfrastructureProject))]
    public void GivenTheWebUiProjectWhenCheckingProjectReferencesThenItShouldOnlyReferenceTheInfrastructureProject() => ArchitectureTestHelper.AssertProjectReferences(
            Path.Combine("src", "WebUi", "WebUi.csproj"),
            Path.Combine("src", "Infrastructure", "Infrastructure.csproj")
        );
}
