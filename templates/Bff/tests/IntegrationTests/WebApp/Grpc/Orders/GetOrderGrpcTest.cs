using CommonTests.Fixtures;
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
        Assert.True(response.Success);
        Assert.True(string.IsNullOrEmpty(response.Message));
        Assert.NotNull(response.Data);
        Assert.Equal(1, response.Data.Id);
        Assert.NotNull(response.Data.Items);
        Assert.NotEmpty(response.Data.Items);
        Assert.Equal(1000.0, response.Data.Total);
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
        Assert.False(response.Success);
        Assert.False(string.IsNullOrEmpty(response.Message));
        Assert.Null(response.Data);
    }
}
