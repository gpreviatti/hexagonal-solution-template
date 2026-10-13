using Domain;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests.Architecture;

public sealed class DomainTests
{
    private static readonly System.Reflection.Assembly s_domainAssembly = typeof(Result).Assembly;

    [Theory(DisplayName = nameof(GivenTheDomainAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheLayerNaming))]
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
    [InlineData("Endpoint")]
    [InlineData("Consumer")]
    [InlineData("Producer")]
    [InlineData("DbContext")]
    [InlineData("Mapping")]
    public void GivenTheDomainAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheLayerNaming(string forbiddenSuffix) => ArchitectureTestHelper.AssertClassNamesDoNotEndWith(s_domainAssembly, forbiddenSuffix);

    [Fact(DisplayName = nameof(GivenTheDomainAssemblyWhenCheckingForbiddenDependenciesThenItShouldNotReferenceOuterLayers))]
    public void GivenTheDomainAssemblyWhenCheckingForbiddenDependenciesThenItShouldNotReferenceOuterLayers() => ArchitectureTestHelper.AssertAssemblyDoesNotReference(s_domainAssembly, "Application", "Infrastructure", "WebApp");

    [Fact(DisplayName = nameof(GivenTheDomainAssemblyWhenCheckingNamespacesThenAllTypesShouldRemainInsideTheDomainRoot))]
    public void GivenTheDomainAssemblyWhenCheckingNamespacesThenAllTypesShouldRemainInsideTheDomainRoot() => ArchitectureTestHelper.AssertNamespacesStartWith(s_domainAssembly, nameof(Domain));

    [Fact(DisplayName = nameof(GivenTheDomainProjectWhenCheckingProjectReferencesThenItShouldNotReferenceAnyOtherProject))]
    public void GivenTheDomainProjectWhenCheckingProjectReferencesThenItShouldNotReferenceAnyOtherProject() => ArchitectureTestHelper.AssertProjectReferences(Path.Combine("src", "Domain", "Domain.csproj"));

    [Fact(DisplayName = nameof(GivenTheDomainDependencyInjectionWhenBuildingTheProviderThenScopesShouldBeValid))]
    public void GivenTheDomainDependencyInjectionWhenBuildingTheProviderThenScopesShouldBeValid()
    {
        ServiceCollection serviceCollection = new();

        serviceCollection.AddDomain();

        var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

        Assert.NotNull(serviceProvider);
    }
}
