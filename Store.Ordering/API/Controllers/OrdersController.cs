using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Ordering.Application.CQRS.Command;
using Store.Ordering.Application.CQRS.Query;

namespace Store.Ordering.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetById(Guid orderId)
    {
        var result = await mediator.Send(new GetOrderByIdQuery(orderId));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var customerIdClaim = User.FindFirst("userId")?.Value;
        if (customerIdClaim == null || !Guid.TryParse(customerIdClaim, out var customerId))
            return Unauthorized();

        var result = await mediator.Send(new GetOrdersByCustomerQuery(customerId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("{orderId:guid}/pay")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Pay(Guid orderId)
    {
        var result = await mediator.Send(new MarkOrderPaidCommand(orderId));
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid orderId)
    {
        var result = await mediator.Send(new CancelOrderCommand(orderId));
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
}
