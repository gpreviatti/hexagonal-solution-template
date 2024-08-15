using WebApp;

namespace IntegrationTests.Common;

[CollectionDefinition("WebApplicationFactoryCollectionDefinition")]
public sealed class WebApplicationFactoryCollectionDefinition : IClassFixture<CustomWebApplicationFactory<Program>>
{
}
