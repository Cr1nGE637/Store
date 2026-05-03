using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Carts.API.Requests;
using Store.Carts.Application.CQRS.Command;
using Store.Carts.Application.CQRS.Query;

namespace Store.Carts.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CartController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId)) return Unauthorized();
        var result = await mediator.Send(new GetCartQuery { CustomerId = customerId }, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId)) return Unauthorized();
        var result = await mediator.Send(new AddItemCommand { CustomerId = customerId, ProductId = request.ProductId, Quantity = request.Quantity }, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId)) return Unauthorized();
        var result = await mediator.Send(new RemoveItemCommand { CustomerId = customerId, CartItemId = cartItemId }, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("items/{cartItemId:guid}/quantity")]
    public async Task<IActionResult> ChangeQuantity(Guid cartItemId, [FromBody] ChangeQuantityRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId)) return Unauthorized();
        var result = await mediator.Send(new ChangeQuantityCommand { CustomerId = customerId, CartItemId = cartItemId, Quantity = request.Quantity }, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId)) return Unauthorized();
        var result = await mediator.Send(new CheckoutCommand { CustomerId = customerId }, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    private bool TryGetCustomerId(out Guid customerId)
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out customerId);
    }
}
