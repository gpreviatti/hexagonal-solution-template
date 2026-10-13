using WebApp;

namespace UnitTests.Architecture;

public sealed class WebAppTests
{
    private static readonly System.Reflection.Assembly s_webAppAssembly = typeof(Program).Assembly;

    [Theory(DisplayName = nameof(GivenTheWebAppAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheEntryPointNaming))]
    [InlineData("Entity")]
    [InlineData("ValueObject")]
    [InlineData("Vo")]
    [InlineData("Repository")]
    [InlineData("DbContext")]
    [InlineData("Mapping")]
    [InlineData("Consumer")]
    [InlineData("Producer")]
    public void GivenTheWebAppAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheEntryPointNaming(string forbiddenSuffix) => ArchitectureTestHelper.AssertClassNamesDoNotEndWith(s_webAppAssembly, forbiddenSuffix);

    [Fact(DisplayName = nameof(GivenTheWebAppAssemblyWhenCheckingNamespacesThenAllTypesShouldRemainInsideTheWebAppRoot))]
    public void GivenTheWebAppAssemblyWhenCheckingNamespacesThenAllTypesShouldRemainInsideTheWebAppRoot() => ArchitectureTestHelper.AssertNamespacesStartWith(s_webAppAssembly, nameof(WebApp), "GrpcOrder");

    [Fact(DisplayName = nameof(GivenTheWebAppProjectWhenCheckingProjectReferencesThenItShouldOnlyReferenceTheInfrastructureProject))]
    public void GivenTheWebAppProjectWhenCheckingProjectReferencesThenItShouldOnlyReferenceTheInfrastructureProject() => ArchitectureTestHelper.AssertProjectReferences(
            Path.Combine("src", "WebApp", "WebApp.csproj"),
            Path.Combine("src", "Infrastructure", "Infrastructure.csproj")
        );
}
