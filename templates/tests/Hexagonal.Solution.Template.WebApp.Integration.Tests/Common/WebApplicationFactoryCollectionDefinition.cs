namespace Hexagonal.Solution.Template.WebApp.Integration.Tests.Common;

[CollectionDefinition("WebApplicationFactoryCollectionDefinition")]
public class WebApplicationFactoryCollectionDefinition : IClassFixture<CustomWebApplicationFactory<Program>>
{
}
