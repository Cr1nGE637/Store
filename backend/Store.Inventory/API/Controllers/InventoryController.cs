using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.Inventory.Application.CQRS.Command;
using Store.Inventory.Application.CQRS.Query;
using Store.Inventory.Application.DTOs;

namespace Store.Inventory.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class InventoryController(IMediator mediator) : ControllerBase
{
    [HttpGet("{productId:guid}")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStock(Guid productId)
    {
        var result = await mediator.Send(new GetStockQuery(productId));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("replenish")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Replenish([FromBody] ReplenishStockCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
}
