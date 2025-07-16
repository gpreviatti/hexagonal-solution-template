using Application.Orders;
using AutoFixture;
using CommonTests.Fixtures;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Common;
using WebApp;

namespace IntegrationTests.WebApp.Orders.Create;
public class CreateOrderTestFixture : BaseFixture
{
    public CustomWebApplicationFactory<Program> customWebApplicationFactory;

    public ApiHelper apiHelper;

    public string RESOURCE_URL = "orders";

    public CreateOrderTestFixture(CustomWebApplicationFactory<Program> customWebApplicationFactory)
    {
        this.customWebApplicationFactory = customWebApplicationFactory;

        apiHelper = new ApiHelper(this.customWebApplicationFactory.CreateClient());
    }

    public CreateOrderRequest SetValidRequest() => autoFixture.Create<CreateOrderRequest>();

    public CreateOrderRequest SetInvalidRequest() => autoFixture
            .Build<CreateOrderRequest>()
            .With(r => r.Description, string.Empty)
            .Create();
}
