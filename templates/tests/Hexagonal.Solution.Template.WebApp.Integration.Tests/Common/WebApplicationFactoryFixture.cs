using Microsoft.AspNetCore.Mvc.Testing;

namespace Hexagonal.Solution.Template.WebApp.Integration.Tests.Common;
public class WebApplicationFactoryFixture
{
    public HttpClient httpClient;
    public ApiFixture apiFixture;
    public WebApplicationFactoryFixture()
    {
        var factory = new WebApplicationFactory<Program>();

        var httpClient = factory.CreateClient();

        apiFixture = new ApiFixture(httpClient);
    }
}
