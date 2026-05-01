using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Carts.API.Requests;
using Store.Carts.Application.CQRS.Command;
using Store.Carts.Application.CQRS.Query;

namespace Store.Carts.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();
        var result = await mediator.Send(new GetCartQuery(customerId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest request, CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();
        var result = await mediator.Send(new AddItemCommand(customerId, request.ProductId, request.Quantity), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();
        var result = await mediator.Send(new RemoveItemCommand(customerId, cartItemId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("items/{cartItemId:guid}/quantity")]
    public async Task<IActionResult> ChangeQuantity(Guid cartItemId, [FromBody] ChangeQuantityRequest request, CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();
        var result = await mediator.Send(new ChangeQuantityCommand(customerId, cartItemId, request.Quantity), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();
        var result = await mediator.Send(new CheckoutCommand(customerId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    private Guid GetCustomerId() => Guid.Parse(User.FindFirst("userId")!.Value);
}
