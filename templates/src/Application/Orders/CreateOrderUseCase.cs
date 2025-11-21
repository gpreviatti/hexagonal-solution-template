using Application.Common.Constants;
using Application.Common.Messages;
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

public sealed class CreateOrderUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<CreateOrderRequest, BaseResponse<OrderDto>, Order, CreateOrderUseCase>(
    serviceProvider,
    serviceProvider.GetRequiredService<IValidator<CreateOrderRequest>>()
)
{
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

            await CreateNotificationAsync(correlationId, "Failed", response, cancellationToken);

            return response;
        }

        var addResult = await _repository.AddAsync(newOrder, correlationId, cancellationToken);
        if (addResult == 0)
        {
            logger.LogWarning(DefaultApplicationMessages.DefaultApplicationMessage + "Failed to create order.", ClassName, HandleMethodName, correlationId);

            response = new(false, null, "Failed to create order.");

            await CreateNotificationAsync(correlationId, "Failed", response, cancellationToken);

            return response;
        }

        response = new(true, new(
            newOrder.Id,
            newOrder.Description,
            newOrder.Total,
            newOrder.CreatedAt,
            [.. newOrder.Items.Select(i => new ItemDto(
                i.Id,
                i.Name,
                i.Description,
                i.Value
            ))]
        ));

        await CreateNotificationAsync(correlationId, "Success", response, cancellationToken);

        return response;
    }

    private async Task CreateNotificationAsync(Guid correlationId, string notificationStatus, object message, CancellationToken cancellationToken) => await _produceService.HandleAsync(
        new CreateNotificationMessage(
            correlationId,
            NotificationType.OrderCreated,
            notificationStatus,
            "System",
            message
        ),
        cancellationToken,
        NotificationType.OrderCreated
    );
}
