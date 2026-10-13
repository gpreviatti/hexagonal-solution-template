using Infrastructure;

namespace UnitTests.Architecture;

public sealed class InfrastructureTests
{
    private static readonly System.Reflection.Assembly s_infrastructureAssembly = typeof(InfrastructureDependencyInjection).Assembly;

    [Theory(DisplayName = nameof(GivenTheInfrastructureAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheAdapterNaming))]
    [InlineData("UseCase")]
    [InlineData("Dto")]
    [InlineData("Request")]
    [InlineData("Response")]
    [InlineData("Endpoint")]
    [InlineData("Controller")]
    public void GivenTheInfrastructureAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheAdapterNaming(string forbiddenSuffix) => ArchitectureTestHelper.AssertClassNamesDoNotEndWith(s_infrastructureAssembly, forbiddenSuffix);

    [Fact(DisplayName = nameof(GivenTheInfrastructureAssemblyWhenCheckingForbiddenDependenciesThenItShouldNotReferenceTheWebLayer))]
    public void GivenTheInfrastructureAssemblyWhenCheckingForbiddenDependenciesThenItShouldNotReferenceTheWebLayer() => ArchitectureTestHelper.AssertAssemblyDoesNotReference(s_infrastructureAssembly, "WebApp");

    [Fact(DisplayName = nameof(GivenTheInfrastructureAssemblyWhenCheckingNamespacesThenAllTypesShouldRemainInsideTheInfrastructureRoot))]
    public void GivenTheInfrastructureAssemblyWhenCheckingNamespacesThenAllTypesShouldRemainInsideTheInfrastructureRoot() => ArchitectureTestHelper.AssertNamespacesStartWith(s_infrastructureAssembly, nameof(Infrastructure));

    [Fact(DisplayName = nameof(GivenTheInfrastructureProjectWhenCheckingProjectReferencesThenItShouldOnlyReferenceTheApplicationProject))]
    public void GivenTheInfrastructureProjectWhenCheckingProjectReferencesThenItShouldOnlyReferenceTheApplicationProject() => ArchitectureTestHelper.AssertProjectReferences(
            Path.Combine("src", "Infrastructure", "Infrastructure.csproj"),
            Path.Combine("src", "Application", "Application.csproj")
        );
}
