using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.Ordering.Application.CQRS.Command;
using Store.Ordering.Application.CQRS.Query;
using Store.Ordering.Application.DTOs;

namespace Store.Ordering.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(GetOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid orderId)
    {
        if (!TryGetRequesterId(out var requesterId)) return Unauthorized();
        var result = await mediator.Send(new GetOrderByIdQuery(orderId, requesterId, IsManager()));
        if (result.IsFailure)
            return result.Error == "Access denied" ? Forbid() : NotFound(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyList<GetOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMy([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (!TryGetRequesterId(out var customerId)) return Unauthorized();
        var result = await mediator.Send(new GetOrdersByCustomerQuery(customerId, page, pageSize));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("{orderId:guid}/pay")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Pay(Guid orderId)
    {
        var result = await mediator.Send(new MarkOrderPaidCommand(orderId));
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("{orderId:guid}/payment")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PayAsCustomer(Guid orderId)
    {
        if (!TryGetRequesterId(out var requesterId)) return Unauthorized();
        var result = await mediator.Send(new MarkOrderPaidCommand(orderId, requesterId));
        if (result.IsFailure)
            return result.Error == "Access denied" ? Forbid() : BadRequest(result.Error);
        return Ok();
    }

    [HttpPost("{orderId:guid}/cancel")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cancel(Guid orderId)
    {
        if (!TryGetRequesterId(out var requesterId)) return Unauthorized();
        var result = await mediator.Send(new CancelOrderCommand(orderId, requesterId));
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
