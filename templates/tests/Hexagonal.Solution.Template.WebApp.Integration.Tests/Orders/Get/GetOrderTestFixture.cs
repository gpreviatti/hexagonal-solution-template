using Hexagonal.Solution.Template.Application.Tests.Common;
using Hexagonal.Solution.Template.Host.WebApp;
using Hexagonal.Solution.Template.WebApp.Integration.Tests.Common;

namespace Hexagonal.Solution.Template.WebApp.Integration.Tests.Orders.Get;

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
