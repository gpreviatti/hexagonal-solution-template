# gRPC Integration Test Template

```csharp
using CommonTests.Fixtures;
using Grpc.Net.Client;
using GrpcOrder;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Grpc.Common;
using WebApp;

namespace IntegrationTests.WebApp.Grpc.{Context};

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class {OperationName}GrpcTest : BaseFixture
{
    public CustomWebApplicationFactory<Program> CustomWebApplicationFactory { get; }
    public ApiGrpcHelper ApiGrpcHelper { get; }

    private readonly GrpcChannel _grpcChannel;
    private readonly {ProtoServiceClient} _service;

    public {OperationName}GrpcTest(CustomWebApplicationFactory<Program> customWebApplicationFactory)
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
        {ProtoRequest} request = new()
        {
            CorrelationId = Guid.NewGuid().ToString(),
            Id = 1
        };

        // Act
        var response = await _service.{Method}Async(request);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.True(string.IsNullOrEmpty(response.Message));
        Assert.NotNull(response.Data);
        Assert.Equal(1, response.Data.Id);
        Assert.NotNull(response.Data.Items);
        Assert.NotEmpty(response.Data.Items);
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFails))]
    public async Task GivenAInvalidRequestThenFails()
    {
        // Arrange
        {ProtoRequest} request = new()
        {
            CorrelationId = Guid.NewGuid().ToString(),
            Id = 999999
        };

        // Act
        var response = await _service.{Method}Async(request);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.False(string.IsNullOrEmpty(response.Message));
        Assert.Null(response.Data);
    }
}
```
