using CommonTests.Fixtures;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcOrder;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Grpc.Common;
using WebApp;

namespace IntegrationTests.WebApp.Grpc.Orders;

[Collection("WebApplicationFactoryCollectionDefinition")]
public class GetOrderGrpcTest : BaseFixture
{
    public CustomWebApplicationFactory<Program> customWebApplicationFactory;

    public ApiGrpcHelper apiGrpcHelper;
    private readonly GrpcChannel _grpcChannel;
    private readonly OrderService.OrderServiceClient _service;

    public GetOrderGrpcTest(CustomWebApplicationFactory<Program> customWebApplicationFactory)
    {
        this.customWebApplicationFactory = customWebApplicationFactory;
        apiGrpcHelper = new(this.customWebApplicationFactory.CreateClient());
        _grpcChannel = apiGrpcHelper.AsGrpcClientChannel();
        _service = new(_grpcChannel);
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Pass))]
    public async Task Given_A_Valid_Request_Then_Pass()
    {
        // Arrange
        GetOrderRequest request = new()
        {
            CorrelationId = Guid.NewGuid().ToString(),
            Id = 1
        };

        // Act
        var response = await _service.GetAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(1, response.Id);
        Assert.Equal("XPTO Client Computers", response.Description);
        Assert.Equal(1000.0, response.Total);
    }

    [Fact(DisplayName = nameof(Given_A_Invalid_Request_Then_Fails))]
    public async Task Given_A_Invalid_Request_Then_Fails()
    {
        // Arrange
        GetOrderRequest request = new()
        {
            CorrelationId = Guid.NewGuid().ToString(),
            Id = 999
        };

        // Act
        var response = await _service.GetAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(0, response.Id);
        Assert.True(string.IsNullOrEmpty(response.Description));
        Assert.Equal(0, response.Total);
    }
}
