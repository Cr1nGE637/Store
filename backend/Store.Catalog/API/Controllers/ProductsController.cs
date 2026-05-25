using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.Catalog.API.Requests;
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
    [ProducesResponseType(typeof(CreateProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateProductDto>> CreateProduct([FromBody] CreateProductRequest request, CancellationToken token)
    {
        var command = new CreateProductCommand
        {
            Sku = request.Sku,
            ProductName = request.ProductName,
            ProductDescription = request.ProductDescription,
            ProductPrice = request.ProductPrice,
            Brand = request.Brand,
            Model = request.Model,
            WarrantyMonths = request.WarrantyMonths,
            CategoryId = request.CategoryId,
            Specifications = request.Specifications
        };
        var result = await _mediator.Send(command, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Created(string.Empty, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GetProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<GetProductDto>>> GetAllProducts([FromQuery] GetProductsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetProductDto>> GetProductById(Guid id, CancellationToken token)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), token);
        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(List<GetProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GetProductDto>>> GetProductsByCategory(
        Guid categoryId,
        CancellationToken token,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _mediator.Send(new GetProductsByCategoryQuery(categoryId, page, pageSize), token);
        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GetProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, CancellationToken token)
    {
        var commandWithId = new UpdateProductCommand
        {
            ProductId = id,
            Sku = request.Sku,
            ProductName = request.ProductName,
            ProductDescription = request.ProductDescription,
            ProductPrice = request.ProductPrice,
            Brand = request.Brand,
            Model = request.Model,
            WarrantyMonths = request.WarrantyMonths,
            CategoryId = request.CategoryId,
            Specifications = request.Specifications
        };
        var result = await _mediator.Send(commandWithId, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(GetProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetProductDto>> DeleteProduct(Guid id, CancellationToken token)
    {
        var result = await _mediator.Send(new DeleteProductCommand { ProductId = id }, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
