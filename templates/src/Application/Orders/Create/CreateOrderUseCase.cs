using Application.Common.Messages;
using Application.Common.UseCases;
using Domain.Orders;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Orders.Create;
public sealed class CreateOrderUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<CreateOrderRequest, OrderDto, Order>(
    serviceProvider,
    serviceProvider.GetService<IValidator<CreateOrderRequest>>()
), ICreateOrderUseCase
{
    private const string ClassName = nameof(CreateOrderUseCase);

    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        string methodName = nameof(HandleInternalAsync);
        Guid correlationId = request.CorrelationId;

        logger.Information("[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Start to execute use case", ClassName, methodName, correlationId);

        var response = new BaseResponse<OrderDto>();

        var items = request.Items
            .Select(i => new Item(default, i.Name, i.Description, i.Value))
            .ToList();

        var newOrder = new Order();
        bool createResult = newOrder.Create(request.Description, items);

        if (!createResult)
        {
            logger.Warning("[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Unable to create order", ClassName, methodName, correlationId);
            response.SetBusinessErrorMessage(ClassName, methodName, correlationId, "Unable to create order");
            return response;
        }

        await _repository.AddAsync(newOrder, cancellationToken);

        response.SetData(new(
            newOrder.Id,
            newOrder.Description,
            newOrder.Total
        ));

        logger.Information("[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Use case was executed with success", ClassName, methodName, correlationId);

        return response;
    }
}
