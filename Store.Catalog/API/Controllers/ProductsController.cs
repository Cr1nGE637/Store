using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Catalog.Application.CQRS.Command;
using Store.Catalog.Application.CQRS.Query;
using Store.Catalog.Application.DTOs;


namespace Store.Catalog.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Manager")]
    [HttpPost]
    public async Task<ActionResult<CreateProductDto>> CreateProduct([FromBody] CreateProductCommand command, CancellationToken token)
    {
        var result = await _mediator.Send(command, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Created(string.Empty, result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<List<GetProductDto>>> GetAllProducts([FromQuery] GetProductsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetProductDto>> GetProductById(Guid id, CancellationToken token)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), token);
        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<ActionResult<List<GetProductDto>>> GetProductsByCategory(Guid categoryId, CancellationToken token)
    {
        var result = await _mediator.Send(new GetProductsByCategoryQuery(categoryId), token);
        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GetProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command, CancellationToken token)
    {
        var commandWithId = new UpdateProductCommand
        {
            ProductId = id,
            ProductName = command.ProductName,
            ProductDescription = command.ProductDescription,
            ProductPrice = command.ProductPrice,
            CategoryId = command.CategoryId
        };
        var result = await _mediator.Send(commandWithId, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<GetProductDto>> DeleteProduct(Guid id, CancellationToken token)
    {
        var result = await _mediator.Send(new DeleteProductCommand { ProductId = id }, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}