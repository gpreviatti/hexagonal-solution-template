using CommonTests.Fixtures;
using IntegrationTests.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Common;

public class BaseHttpFixture : BaseFixture
{
    public ApiHelper ApiHelper { get; set; } = null!;
    public string ResourceUrl { get; set; } = string.Empty;

    public void SetApiHelper(CustomWebApplicationFactory<Program> customWebApplicationFactory) =>
        ApiHelper = new(customWebApplicationFactory.CreateClient());
}
