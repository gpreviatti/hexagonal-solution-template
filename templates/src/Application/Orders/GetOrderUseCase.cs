using Application.Common.Constants;
using Application.Common.Requests;
using Application.Common.UseCases;
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
        var order = await _repository.GetByIdAsNoTrackingAsync<Order, OrderDto>(
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
            logger.LogWarning(
                DefaultApplicationMessages.DefaultApplicationMessage + "Order not found.",
                ClassName, HandleMethodName, request.CorrelationId
            );
            return new(false, null, "Order not found.");
        }

        return new(true, order);
    }
}
