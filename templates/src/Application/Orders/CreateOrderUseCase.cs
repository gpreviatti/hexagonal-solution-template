using Application.Common.Constants;
using Application.Common.Messages;
using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Orders;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Orders;

public sealed record CreateOrderRequest(Guid CorrelationId, string Description, CreateOrderItemRequest[] Items) : BaseRequest(CorrelationId);

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
    private readonly IBaseRepository<Order> _repository = serviceProvider
        .GetRequiredService<IBaseRepository<Order>>();

    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        Guid correlationId = request.CorrelationId;
        BaseResponse<OrderDto> response;

        var items = request.Items
            .Select(i => new Item(i.Name, i.Description, i.Value))
            .ToList();

        var newOrder = new Order(request.Description, items);
        var createResult = newOrder.SetTotal();
        if (createResult.IsFailure)
        {
            logger.LogWarning(DefaultApplicationMessages.DefaultApplicationMessage + createResult.Message, ClassName, HandleMethodName, correlationId);

            response = new(false, null, createResult.Message);

            CreateNotification(correlationId, "Failed", response);

            return response;
        }

        var addResult = await _repository.AddAsync(newOrder, correlationId, cancellationToken);
        if (addResult == 0)
        {
            logger.LogWarning(DefaultApplicationMessages.DefaultApplicationMessage + "Failed to create order.", ClassName, HandleMethodName, correlationId);

            response = new(false, null, "Failed to create order.");

            CreateNotification(correlationId, "Failed", response);

            return response;
        }

        response = new(true, new OrderDto()
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

        CreateNotification(correlationId, "Success", response);

        return response;
    }

    private void CreateNotification(Guid correlationId, string notificationStatus, object message) => _ = _produceService.HandleAsync(
        new CreateNotificationMessage(
            correlationId,
            NotificationType.OrderCreated,
            notificationStatus,
            "System",
            message
        ),
        CancellationToken.None,
        NotificationType.OrderCreated
    );
}
