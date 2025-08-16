using System.Diagnostics.Metrics;
using Application.Common.Constants;
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

public sealed class CreateOrderUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<CreateOrderRequest, OrderDto, Order, CreateOrderUseCase>(
    serviceProvider,
    serviceProvider.GetService<IValidator<CreateOrderRequest>>()
)
{
    private const string ClassName = nameof(CreateOrderUseCase);
    public static Counter<int> OrderCreated = DefaultConfigurations.Meter
        .CreateCounter<int>("order.created", "orders", "Number of orders created");

    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        string methodName = nameof(HandleInternalAsync);
        Guid correlationId = request.CorrelationId;

        var response = new BaseResponse<OrderDto>();

        var items = request.Items
            .Select(i => new Item(default, i.Name, i.Description, i.Value))
            .ToList();

        var newOrder = new Order();
        var createResult = newOrder.Create(request.Description, items);

        if (createResult.IsFailure)
        {
            logger.LogWarning(DefaultApplicationMessages.DefaultApplicationMessage + createResult.Message, ClassName, methodName, correlationId);
            response.SetMessage(createResult.Message);
            return response;
        }

        var addResult = await _repository.AddAsync(newOrder, cancellationToken);

        if (addResult == 0)
        {
            logger.LogWarning(DefaultApplicationMessages.DefaultApplicationMessage + "Failed to create order.", ClassName, methodName, correlationId);
            response.SetMessage("Failed to create order.");
            return response;
        }

        response.SetData(new(
            newOrder.Id,
            newOrder.Description,
            newOrder.Total
        ));

        logger.LogInformation(DefaultApplicationMessages.FinishedExecutingUseCase, ClassName, methodName, correlationId);

        OrderCreated.Add(1);

        return response;
    }
}
