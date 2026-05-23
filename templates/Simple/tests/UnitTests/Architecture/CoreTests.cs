using Core;
using Core.Common.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests.Architecture;

public sealed class CoreTests
{
    private static readonly System.Reflection.Assembly _CoreAssembly = typeof(BaseResponse).Assembly;

    [Theory(DisplayName = nameof(GivenTheCoreAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheLayerNaming))]
    [InlineData("Controller")]
    [InlineData("Endpoint")]
    [InlineData("DbContext")]
    [InlineData("Mapping")]
    [InlineData("Consumer")]
    [InlineData("Producer")]
    public void GivenTheCoreAssemblyWhenCheckingForbiddenClassSuffixesThenItShouldRespectTheLayerNaming(string forbiddenSuffix) => ArchitectureTestHelper.AssertClassNamesDoNotEndWith(_CoreAssembly, forbiddenSuffix);

    [Fact(DisplayName = nameof(GivenTheCoreAssemblyWhenCheckingForbiddenDependenciesThenItShouldNotReferenceOuterLayers))]
    public void GivenTheCoreAssemblyWhenCheckingForbiddenDependenciesThenItShouldNotReferenceOuterLayers() => ArchitectureTestHelper.AssertAssemblyDoesNotReference(_CoreAssembly, "Infrastructure", "WebApp");

    [Fact(DisplayName = nameof(GivenTheCoreAssemblyWhenCheckingNamespacesThenAllTypesShouldRemainInsideTheCoreRoot))]
    public void GivenTheCoreAssemblyWhenCheckingNamespacesThenAllTypesShouldRemainInsideTheCoreRoot() => ArchitectureTestHelper.AssertNamespacesStartWith(_CoreAssembly, nameof(Core));

    [Fact(DisplayName = nameof(GivenTheCoreProjectWhenCheckingProjectReferencesThenItShouldNotReferenceAnyOtherProject))]
    public void GivenTheCoreProjectWhenCheckingProjectReferencesThenItShouldNotReferenceAnyOtherProject() => ArchitectureTestHelper.AssertProjectReferences(Path.Combine("src", "Core", "Core.csproj"));

    [Fact(DisplayName = nameof(GivenTheCoreDependencyInjectionWhenBuildingTheProviderThenScopesShouldBeValid))]
    public void GivenTheCoreDependencyInjectionWhenBuildingTheProviderThenScopesShouldBeValid()
    {
        ServiceCollection serviceCollection = new();

        serviceCollection.AddCore();

        var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

        Assert.NotNull(serviceProvider);
    }
}
