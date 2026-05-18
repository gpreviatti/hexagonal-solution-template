using System.ComponentModel.DataAnnotations;
using Application.Common.Attributes;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Common.Enums;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders;

public sealed record UpdateOrderRequest(
    Guid CorrelationId,
    [property: NotDefault] int OrderId,
    [property: Required] string Description,
    [property: MinLength(1, ErrorMessage = "At least one item is required")] UpdateOrderItemRequest[] Items,
    string ModifiedBy = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, ModifiedBy, TimezoneId);

public sealed record UpdateOrderItemRequest(
    [property: Required] string Name,
    string Description,
    [property: Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0")] decimal Value
);

public sealed class UpdateOrderUseCase(IServiceProvider serviceProvider)
    : BaseInOutUseCase<UpdateOrderRequest, BaseResponse<OrderDto>>(serviceProvider)
{
    private readonly NotificationType _notificationType = NotificationType.OrderUpdated;

    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        UpdateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var correlationId = request.CorrelationId;
        BaseResponse<OrderDto> response;

        var order = await Repository.GetQueryable<Order>(correlationId)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

        if (order is null)
            return HandleFailedResponse<UpdateOrderRequest, BaseResponse<OrderDto>>(
                request, correlationId, _notificationType,
                request.ModifiedBy, "Order not found."
            );

        var items = request.Items
            .Select(i => new Item(i.Name, i.Description, i.Value))
            .ToList();

        var updateResult = order.Update(request.Description, items, request.ModifiedBy, request.TimezoneId);
        if (updateResult.IsFailure)
            return HandleFailedResponse<UpdateOrderRequest, BaseResponse<OrderDto>>(
                request, correlationId, _notificationType,
                request.ModifiedBy, updateResult.Message
            );

        if (await Repository.UpdateAsync(order, correlationId, cancellationToken) == 0)
            return HandleFailedResponse<UpdateOrderRequest, BaseResponse<OrderDto>>(
                request, correlationId, _notificationType,
                request.ModifiedBy, "Failed to update order."
            );

        response = new(true, new()
        {
            Id = order.Id,
            Description = order.Description,
            Total = order.Total,
            PeriodSinceWasCreated = order.GetPeriodSinceWasCreated(),
            Items = [.. order.Items.Select(i => new ItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Value = i.Value
            })]
        });

        HandleNotification(correlationId, NotificationStatus.Success, request.ModifiedBy, _notificationType, response);

        return response;
    }
}
