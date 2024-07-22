using CommonTests.Fixtures;
using IntegrationTests.WebApp.Common;
using WebApp;

namespace IntegrationTests.WebApp.Orders.Get;

public class GetOrderTestFixture : BaseFixture
{
    public CustomWebApplicationFactory<Program> customWebApplicationFactory;

    public ApiHelper apiHelper;

    public string RESOURCE_URL = "orders";

    public GetOrderTestFixture(CustomWebApplicationFactory<Program> customWebApplicationFactory)
    {
        this.customWebApplicationFactory = customWebApplicationFactory;

        apiHelper = new ApiHelper(this.customWebApplicationFactory.CreateClient());
    }
}
