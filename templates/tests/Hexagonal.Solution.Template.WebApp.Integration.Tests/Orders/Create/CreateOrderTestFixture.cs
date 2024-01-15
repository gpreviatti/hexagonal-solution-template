using AutoFixture;
using Hexagonal.Solution.Template.Application.Orders.Create;
using Hexagonal.Solution.Template.Application.Tests.Common;
using Hexagonal.Solution.Template.Host.WebApp;
using Hexagonal.Solution.Template.WebApp.Integration.Tests.Common;

namespace Hexagonal.Solution.Template.WebApp.Integration.Tests.Orders.Create;
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
