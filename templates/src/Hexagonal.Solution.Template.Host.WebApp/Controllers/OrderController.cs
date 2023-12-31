using Hexagonal.Solution.Template.Application.Common.Messages;
using Hexagonal.Solution.Template.Application.Orders.Create;
using Hexagonal.Solution.Template.Application.Orders.Get;
using Hexagonal.Solution.Template.Application.Orders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Hexagonal.Solution.Template.Host.WebApp.Controllers;
[ApiController]
[Route("orders")]
public class OrderController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<BaseResponse<OrderDto>> Get([FromRoute] int id)
    {
        var request = new GetOrderRequest(id);

        var response = await _mediator.Send(request);

        return response;
    }

    [HttpPost()]
    public async Task<CreatedResult> Create([FromBody] CreateOrderRequest request)
    {
        var response = await _mediator.Send(request);

        return Created("", response);
    }
}
