using MediatR;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Ordering.Commands.PayOrder;


namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders/commands")]
public class OrdersCommandController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersCommandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{orderId:guid}/pay")]
    public async Task<IActionResult> Pay(Guid orderId)
    {
        await _mediator.Send(new PayOrderCommand(orderId));
        return NoContent();
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid orderId)
    {
        await _mediator.Send(new CancelOrderCommand(orderId));
        return NoContent();
    }
}
