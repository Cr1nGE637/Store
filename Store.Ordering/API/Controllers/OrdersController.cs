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
        if (!TryGetRequesterId(out var requesterId)) return Unauthorized();
        var result = await mediator.Send(new GetOrderByIdQuery(orderId, requesterId, IsManager()));
        if (result.IsFailure)
            return result.Error == "Access denied" ? Forbid() : NotFound(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        if (!TryGetRequesterId(out var customerId)) return Unauthorized();
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
        if (!TryGetRequesterId(out var requesterId)) return Unauthorized();
        var result = await mediator.Send(new CancelOrderCommand(orderId, requesterId, IsManager()));
        if (result.IsFailure)
            return result.Error == "Access denied" ? Forbid() : BadRequest(result.Error);
        return Ok();
    }

    private bool TryGetRequesterId(out Guid requesterId)
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out requesterId);
    }

    private bool IsManager() => User.IsInRole("Manager");
}
