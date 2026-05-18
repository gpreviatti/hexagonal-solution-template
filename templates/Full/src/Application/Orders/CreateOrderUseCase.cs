using System.ComponentModel.DataAnnotations;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Common.Enums;
using Domain.Orders;

namespace Application.Orders;

public sealed record CreateOrderRequest(
    Guid CorrelationId,
    string Description,
    [property: MinLength(1, ErrorMessage = "At least one item is required")] CreateOrderItemRequest[] Items,
    string CreatedBy = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, CreatedBy, TimezoneId);

public sealed record CreateOrderItemRequest([property: Required] string Name, string Description, [property: Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0")] decimal Value);

public sealed class CreateOrderUseCase(IServiceProvider serviceProvider)
    : BaseInOutUseCase<CreateOrderRequest, BaseResponse<OrderDto>>(serviceProvider)
{
    private readonly NotificationType _notificationType = NotificationType.OrderCreated;
    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var correlationId = request.CorrelationId;
        BaseResponse<OrderDto> response;

        var items = request.Items
            .Select(i => new Item(i.Name, i.Description, i.Value))
            .ToList();

        var createResult = Order.Create(
            request.Description, items,
            request.CreatedBy, request.TimezoneId
        );
        if (createResult.IsFailure)
            return HandleFailedResponse<CreateOrderRequest, BaseResponse<OrderDto>>(
                request, correlationId, _notificationType,
                request.CreatedBy, createResult.Message
            );

        var newOrder = createResult.Value;
        if (await Repository.AddAsync(newOrder, correlationId, cancellationToken) == 0)
            return HandleFailedResponse<CreateOrderRequest, BaseResponse<OrderDto>>(
                request, correlationId, _notificationType,
                request.CreatedBy, "Failed to create order."
            );

        response = new(true, new()
        {
            Id = newOrder.Id,
            Total = newOrder.Total,
            PeriodSinceWasCreated = newOrder.GetPeriodSinceWasCreated(),
            Items = [.. newOrder.Items.Select(i => new ItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Value = i.Value
            })]
        });

        HandleNotification(correlationId, NotificationStatus.Success, request.CreatedBy, _notificationType, response);

        return response;
    }
}
