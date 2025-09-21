using CommonTests.Fixtures;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcOrder;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Grpc.Common;
using WebApp;
using WebApp.GrpcServices;

namespace IntegrationTests.WebApp.Grpc.Orders;

[Collection("WebApplicationFactoryCollectionDefinition")]
public class GetOrderGrpcTest : BaseFixture
{
    public CustomWebApplicationFactory<Program> customWebApplicationFactory;

    public ApiGrpcHelper apiGrpcHelper;

    public const string RESOURCE_URL = "orders/paginated";
    private readonly CallInvoker _callInvoker;

    static readonly Marshaller<GetOrderRequest> __Marshaller_OrderRequest = Marshallers.Create(Google.Protobuf.MessageExtensions.ToByteArray, GetOrderRequest.Parser.ParseFrom);
    static readonly Marshaller<OrderReply> __Marshaller_OrderReply = Marshallers.Create(Google.Protobuf.MessageExtensions.ToByteArray, OrderReply.Parser.ParseFrom);

    static Method<GetOrderRequest, OrderReply> __Method_Get = new(
        MethodType.Unary,
        "OrderService",
        "Get",
        __Marshaller_OrderRequest,
        __Marshaller_OrderReply
    );


    public GetOrderGrpcTest(CustomWebApplicationFactory<Program> customWebApplicationFactory)
    {
        this.customWebApplicationFactory = customWebApplicationFactory;
        apiGrpcHelper = new(this.customWebApplicationFactory.CreateClient());

        var grpcChannel = apiGrpcHelper.AsGrpcClientChannel();
        _callInvoker = grpcChannel.CreateCallInvoker();
    }

    [Fact]
    public async Task GetUnaryTest()
    {
        // Arrange
        GetOrderRequest request = new()
        {
            CorrelationId = Guid.NewGuid().ToString(),
            Id = 1
        };
        CallOptions callOptions = new(null, DateTime.MaxValue, cancellationToken);

        // Act
        var reply = _callInvoker.AsyncUnaryCall(
            __Method_Get,
            string.Empty,
            callOptions,
            request
        );
        var response = await reply.ResponseAsync;


        // Assert
        Assert.NotNull(response);
    }
}
