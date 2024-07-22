using WebApp;

namespace IntegrationTests.WebApp.Common;

[CollectionDefinition("WebApplicationFactoryCollectionDefinition")]
public sealed class WebApplicationFactoryCollectionDefinition : IClassFixture<CustomWebApplicationFactory<Program>>
{
}
