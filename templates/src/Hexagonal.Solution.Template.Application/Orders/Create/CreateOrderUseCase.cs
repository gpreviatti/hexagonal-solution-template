﻿using FluentValidation;
using Hexagonal.Solution.Template.Application.Common.Messages;
using Hexagonal.Solution.Template.Application.Common.UseCases;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Domain.Orders.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hexagonal.Solution.Template.Application.Orders.Create;
public sealed class CreateOrderUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<CreateOrderRequest, OrderDto>(
    serviceProvider,
    serviceProvider.GetService<IValidator<CreateOrderRequest>>()
), ICreateOrderUseCase
{
    private readonly ICreateOrderService _createOrderService = serviceProvider.GetService<ICreateOrderService>();
    private readonly IOrderRepository _orderRepository = serviceProvider.GetService<IOrderRepository>();

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

        await _orderRepository.AddAsync(newOrder.Value, cancellationToken);

        response.SetData(new(
            newOrder.Value!.Id,
            newOrder.Value.Description,
            newOrder.Value.Total
        ));

        logger.Information("Use case was executed with success", response);

        return response;
    }
}
