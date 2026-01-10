using IntegrationTests.Common;
using IntegrationTests.Fixtures;
using WebApp;

namespace IntegrationTests.WebApp.Http;

public class BaseHttpFixture : BaseFixture
{
    public ApiHelper apiHelper;
    public string resourceUrl = string.Empty;

    public void SetApiHelper(CustomWebApplicationFactory<Program> customWebApplicationFactory) =>
        apiHelper = new(customWebApplicationFactory.CreateClient());
}