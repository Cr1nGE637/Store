using MediatR;
using Microsoft.AspNetCore.Mvc;
using Store.Application.CQRS.Customers.Command;
using Store.Application.CQRS.Customers.Query;
using Store.Application.CQRS.Products.Command;
using Store.Application.CQRS.Products.Query;
using Store.Application.DTOs;

namespace Store.API.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CreateCustomerDto>> CreateCustomer(CreateCustomerCommand command,  CancellationToken token)
    {
        var result = await _mediator.Send(command, token);

        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Created(string.Empty, result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<List<GetCustomerDto>>> GetAllCustomers([FromQuery] GetCustomersQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }
    
    [HttpPut]
    public async Task<ActionResult<GetCustomerDto>> UpdateCustomer([FromBody] UpdateCustomerCommand command,  CancellationToken token)
    {
        var result = await _mediator.Send(command, token);
        
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }

    [HttpDelete]
    public async Task<ActionResult<GetProductDto>> DeleteCustomer([FromQuery] DeleteCustomerCommand command, CancellationToken token)
    {
        var result = await _mediator.Send(command, token);
        
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }
}