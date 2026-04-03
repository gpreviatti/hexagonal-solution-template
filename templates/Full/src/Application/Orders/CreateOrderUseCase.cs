using Application.Common.Helpers;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Common.Enums;
using Domain.Orders;
using FluentValidation;

namespace Application.Orders;

public sealed record CreateOrderRequest(
    Guid CorrelationId,
    string Description,
    CreateOrderItemRequest[] Items,
    string CreatedBy = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, CreatedBy, TimezoneId);

public sealed record CreateOrderItemRequest(string Name, string Description, decimal Value);

public sealed class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.Value).NotEmpty();
    }
}

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(r => r.CorrelationId).NotEmpty();
        RuleFor(r => r.Description).NotEmpty();
        RuleFor(r => r.Items).NotEmpty();
        RuleForEach(r => r.Items).SetValidator(new CreateOrderItemRequestValidator());
    }
}

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
        {
            Logs.FailedOperation(Logger, correlationId, createResult.Message);

            response = new(false, null, createResult.Message);

            CreateNotification(correlationId, "Failed", request.CreatedBy, _notificationType, response);

            UseCaseFailedMetric.Add(1);

            return response;
        }

        var newOrder = createResult.Value;
        if (await Repository.AddAsync(newOrder, correlationId, cancellationToken) == 0)
        {
            Logs.FailedOperation(Logger, correlationId, "Failed to create order. No rows affected.");

            response = new(false, null, "Failed to create order.");

            CreateNotification(correlationId, "Failed", request.CreatedBy, _notificationType, response);

            UseCaseFailedMetric.Add(1);

            return response;
        }

        response = new(true, new()
        {
            Id = newOrder.Id,
            Total = newOrder.Total,
            Items = [.. newOrder.Items.Select(i => new ItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Value = i.Value
            })]
        });

        CreateNotification(correlationId, "Success", request.CreatedBy, _notificationType, response);

        return response;
    }
}
