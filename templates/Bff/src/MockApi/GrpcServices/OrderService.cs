using AutoFixture;
using Grpc.Core;
using GrpcOrder;
using static GrpcOrder.OrderService;

namespace MockApi.GrpcServices;

public class OrderService : OrderServiceBase
{
    public override async Task<OrderReply> Get(
        GetOrderRequest request,
        ServerCallContext context
    ) => new Fixture().Create<OrderReply>();
}
