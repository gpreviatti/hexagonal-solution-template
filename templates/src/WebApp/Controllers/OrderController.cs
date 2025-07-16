using Application.Common.Messages;
using Application.Orders;
using Application.Orders.Create;
using Application.Orders.Get;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;
[ApiController]
[Route("orders")]
public sealed class OrderController(IServiceProvider serviceProvider) : ControllerBase
{
    private readonly IGetOrderUserCase _getOrderUserCase = serviceProvider.GetRequiredService<IGetOrderUserCase>();
    private readonly ICreateOrderUseCase _createOrderUseCase = serviceProvider.GetRequiredService<ICreateOrderUseCase>();

    /// <summary>
    /// Get a specific order
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/{correlationId}")]
    [ProducesResponseType<BaseResponse<OrderDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<BaseResponse<OrderDto>>(StatusCodes.Status400BadRequest)]
    public async Task<OkObjectResult> Get([FromRoute] int id, [FromRoute] Guid correlationId)
    {
        var request = new GetOrderRequest(correlationId, id);

        var response = await _getOrderUserCase.Handle(request, CancellationToken.None);

        return Ok(response);
    }

    /// <summary>
    /// Create order
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [ProducesResponseType<BaseResponse<OrderDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<BaseResponse<OrderDto>>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<CreatedResult> Create([FromBody] CreateOrderRequest request)
    {
        var response = await _createOrderUseCase.Handle(request, CancellationToken.None);

        return Created($"/orders/{response.Data.Id}", response);
    }
}
