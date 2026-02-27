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
    public CustomWebApplicationFactory<Program> CustomWebApplicationFactory { get; }

    public ApiGrpcHelper ApiGrpcHelper { get; }
    private readonly GrpcChannel _grpcChannel;
    private readonly OrderService.OrderServiceClient _service;

    public GetOrderGrpcTest(CustomWebApplicationFactory<Program> customWebApplicationFactory)
    {
        CustomWebApplicationFactory = customWebApplicationFactory;
        ApiGrpcHelper = new(CustomWebApplicationFactory.CreateClient());
        _grpcChannel = ApiGrpcHelper.AsGrpcClientChannel();
        _service = new(_grpcChannel);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
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
        Assert.Equal(2000.0, response.Data.Total);
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFails))]
    public async Task GivenAInvalidRequestThenFails()
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
