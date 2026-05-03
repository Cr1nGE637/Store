using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Inventory.Application.CQRS.Command;
using Store.Inventory.Application.CQRS.Query;

namespace Store.Inventory.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class InventoryController(IMediator mediator) : ControllerBase
{
    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> GetStock(Guid productId)
    {
        var result = await mediator.Send(new GetStockQuery(productId));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("replenish")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Replenish([FromBody] ReplenishStockCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
}
