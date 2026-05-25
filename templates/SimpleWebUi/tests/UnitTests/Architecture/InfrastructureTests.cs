using Infrastructure;

namespace UnitTests.Architecture;

public sealed class InfrastructureTests
{
    private static readonly System.Reflection.Assembly _assembly = typeof(InfrastructureDependencyInjection).Assembly;

    [Theory(DisplayName = nameof(GivenTheInfrastructureAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheAdapterNaming))]
    [InlineData("UseCase")]
    [InlineData("Dto")]
    [InlineData("Request")]
    [InlineData("Response")]
    [InlineData("Endpoint")]
    [InlineData("Controller")]
    public void GivenTheInfrastructureAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheAdapterNaming(string forbiddenSuffix) => ArchitectureTestHelper.AssertClassNamesDoNotEndWith(_assembly, forbiddenSuffix);

    [Fact(DisplayName = nameof(GivenTheInfrastructureAssemblyWhenCheckingForbiddenDependenciesThenItShouldNotReferenceTheWebLayer))]
    public void GivenTheInfrastructureAssemblyWhenCheckingForbiddenDependenciesThenItShouldNotReferenceTheWebLayer() => ArchitectureTestHelper.AssertAssemblyDoesNotReference(_assembly, "WebUi");

    [Fact(DisplayName = nameof(GivenTheInfrastructureAssemblyWhenCheckingNamespacesThenAllTypesShouldRemainInsideTheInfrastructureRoot))]
    public void GivenTheInfrastructureAssemblyWhenCheckingNamespacesThenAllTypesShouldRemainInsideTheInfrastructureRoot() => ArchitectureTestHelper.AssertNamespacesStartWith(_assembly, nameof(Infrastructure));

    [Fact(DisplayName = nameof(GivenTheInfrastructureProjectWhenCheckingProjectReferencesThenItShouldOnlyReferenceTheCoreProject))]
    public void GivenTheInfrastructureProjectWhenCheckingProjectReferencesThenItShouldOnlyReferenceTheCoreProject() => ArchitectureTestHelper.AssertProjectReferences(
            Path.Combine("src", "Infrastructure", "Infrastructure.csproj"),
            Path.Combine("src", "Core", "Core.csproj")
        );
}
