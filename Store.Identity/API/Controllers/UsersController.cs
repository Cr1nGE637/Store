using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Application.CQRS.Command;
using Users.Application.CQRS.Query;
using Users.Application.DTOs;

namespace Users.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterDto>> Register([FromBody] RegisterCommand command, CancellationToken token)
    {
        var result = await _mediator.Send(command, token);
        
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginDto>> Login([FromBody] LoginCommand command, CancellationToken token)
    {
        var result = await _mediator.Send(command, token);
        if (result.IsSuccess)
        {
            Response.Cookies.Append("tasty-cookies", result.Value.Token);
        }
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<GetUserDto>> GetUserByEmail([FromQuery] GetUserQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }
    
}