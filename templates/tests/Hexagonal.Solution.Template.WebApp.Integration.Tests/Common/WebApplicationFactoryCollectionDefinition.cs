using Hexagonal.Solution.Template.Host.WebApp;

namespace Hexagonal.Solution.Template.WebApp.Integration.Tests.Common;

[CollectionDefinition("WebApplicationFactoryCollectionDefinition")]
public sealed class WebApplicationFactoryCollectionDefinition : IClassFixture<CustomWebApplicationFactory<Program>>
{
}
