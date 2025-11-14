using MediatR;
using Microsoft.AspNetCore.Mvc;
using Store.Application.CQRS.Products.Command;
using Store.Application.CQRS.Products.Query;
using Store.Application.DTOs;
using Store.Domain.Entities;

namespace Store.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CreateProductDto>> CreateProduct(CreateProductCommand command,  CancellationToken token)
    {
        var result = await _mediator.Send(command, token);

        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Created(string.Empty, result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<List<GetProductDto>>> GetAllProductsAsync([FromQuery] GetProductsQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpPut]
    public async Task<ActionResult<GetProductDto>> UpdateProduct([FromQuery] UpdateProductCommand command,  CancellationToken token)
    {
        var result = await _mediator.Send(command, token);
        
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpDelete]
    public async Task<ActionResult<GetProductDto>> DeleteProduct([FromQuery] DeleteProductCommand command, CancellationToken token)
    {
        var result = await _mediator.Send(command, token);
        
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }
}