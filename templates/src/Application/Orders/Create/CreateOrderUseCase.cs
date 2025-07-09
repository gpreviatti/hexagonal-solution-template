using Application.Common.Messages;
using Application.Common.UseCases;
using Domain.Orders;
using Domain.Orders.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Orders.Create;
public sealed class CreateOrderUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<CreateOrderRequest, OrderDto, Order>(
    serviceProvider,
    serviceProvider.GetService<IValidator<CreateOrderRequest>>()
), ICreateOrderUseCase
{
    private readonly ICreateOrderService _createOrderService = serviceProvider.GetRequiredService<ICreateOrderService>();

    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = new BaseResponse<OrderDto>();

        var items = request.Items
            .Select(i => new Item(default, i.Name, i.Description, i.Value))
            .ToList();

        var newOrder = _createOrderService.Handle(
            request.Description,
            items
        );

        if (newOrder.IsFailure)
        {
            logger.Error(newOrder.Message, request);
            response.SetBusinessErrorMessage(newOrder.Message);
            return response;
        }

        await _repository.AddAsync(newOrder.Value, cancellationToken);

        response.SetData(new(
            newOrder.Value!.Id,
            newOrder.Value.Description,
            newOrder.Value.Total
        ));

        logger.Information("Use case was executed with success", response);

        return response;
    }
}
