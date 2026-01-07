using CommonTests.Fixtures;
using IntegrationTests.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Common;

public class BaseHttpFixture : BaseFixture
{
    public ApiHelper apiHelper;
    public string resourceUrl = string.Empty;

    public void SetApiHelper(CustomWebApplicationFactory<Program> customWebApplicationFactory) =>
        apiHelper = new(customWebApplicationFactory.CreateClient());
}