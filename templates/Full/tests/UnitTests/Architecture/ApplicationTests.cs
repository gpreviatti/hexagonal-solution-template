using Application;
using Application.Common.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests.Architecture;

public sealed class ApplicationTests
{
    private static readonly System.Reflection.Assembly _applicationAssembly = typeof(BaseResponse).Assembly;

    [Theory(DisplayName = nameof(GivenTheApplicationAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheLayerNaming))]
    [InlineData("Entity")]
    [InlineData("ValueObject")]
    [InlineData("Vo")]
    [InlineData("Service")]
    [InlineData("Controller")]
    [InlineData("Endpoint")]
    [InlineData("DbContext")]
    [InlineData("Mapping")]
    [InlineData("Consumer")]
    [InlineData("Producer")]
    public void GivenTheApplicationAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheLayerNaming(string forbiddenSuffix) => ArchitectureTestHelper.AssertClassNamesDoNotEndWith(_applicationAssembly, forbiddenSuffix);

    [Fact(DisplayName = nameof(GivenTheApplicationAssemblyWhenCheckingForbiddenDependenciesThenItShouldNotReferenceOuterLayers))]
    public void GivenTheApplicationAssemblyWhenCheckingForbiddenDependenciesThenItShouldNotReferenceOuterLayers() => ArchitectureTestHelper.AssertAssemblyDoesNotReference(_applicationAssembly, "Infrastructure", "WebApp");

    [Fact(DisplayName = nameof(GivenTheApplicationAssemblyWhenCheckingNamespacesThenAllTypesShouldRemainInsideTheApplicationRoot))]
    public void GivenTheApplicationAssemblyWhenCheckingNamespacesThenAllTypesShouldRemainInsideTheApplicationRoot() => ArchitectureTestHelper.AssertNamespacesStartWith(_applicationAssembly, nameof(Application));

    [Fact(DisplayName = nameof(GivenTheApplicationProjectWhenCheckingProjectReferencesThenItShouldOnlyReferenceTheDomainProject))]
    public void GivenTheApplicationProjectWhenCheckingProjectReferencesThenItShouldOnlyReferenceTheDomainProject() => ArchitectureTestHelper.AssertProjectReferences(
            Path.Combine("src", "Application", "Application.csproj"),
            Path.Combine("src", "Domain", "Domain.csproj")
        );

    [Fact(DisplayName = nameof(GivenTheApplicationDependencyInjectionWhenBuildingTheProviderThenScopesShouldBeValid))]
    public void GivenTheApplicationDependencyInjectionWhenBuildingTheProviderThenScopesShouldBeValid()
    {
        ServiceCollection serviceCollection = new();

        serviceCollection.AddApplication();

        var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

        Assert.NotNull(serviceProvider);
    }
}
