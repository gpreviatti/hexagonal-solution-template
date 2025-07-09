using Application.Common.Messages;
using MediatR;

namespace Application.Orders.Get;

public sealed record GetOrderRequest(Guid CorrelationId, int Id) : BaseRequest(CorrelationId), IRequest<BaseResponse<OrderDto>>;
