using FluentValidation;
using Hexagonal.Solution.Template.Application.Common;
using Hexagonal.Solution.Template.Application.Common.Messages;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Domain.Orders.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hexagonal.Solution.Template.Application.Orders.Create;
public class CreateOrderUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<CreateOrderRequest, OrderDto>(
    serviceProvider,
    serviceProvider.GetService<IValidator<CreateOrderRequest>>()
), ICreateOrderUseCase
{
    private readonly ICreateOrderService _createOrderService = serviceProvider.GetService<ICreateOrderService>();

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

        response.SetData(
            new(newOrder.Value!.Id)
        );
        logger.Information("Use case was executed with success", response);

        return await Task.FromResult(response);
    }
}
