using Application.Common.Requests;
using Application.Common.UseCases;
using Application.Common.Helpers;
using Domain.Orders;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Orders;

public sealed record GetOrderRequest(Guid CorrelationId, int Id) : BaseRequest(CorrelationId);

public sealed class GetOrderRequestValidator : AbstractValidator<GetOrderRequest>
{
    public GetOrderRequestValidator()
    {
        RuleFor(r => r.Id).NotEmpty();
    }
}

public sealed class GetOrderUseCase(IServiceProvider serviceProvider)  : BaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>>(serviceProvider)
{
    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        GetOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var order = await Repository.GetByIdAsNoTrackingAsync<Order, OrderDto>(
            request.Id,
            request.CorrelationId,
            o => new OrderDto()
            {
                Id = o.Id,
                Total = o.Total,
                Items = o.Items.Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Value = i.Value
                }).ToArray()
            },
            cancellationToken
        );

        if (order is null)
        {
            Logs.NotFound(Logger, ClassName, HandleMethodName, request.CorrelationId, nameof(order));
            return new(false, null, "Order not found.");
        }

        return new(true, order);
    }
}
