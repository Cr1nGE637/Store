using Store.Identity.Application.CQRS.Command;
using Store.Identity.Application.CQRS.Query;
using Store.Identity.Application.DTOs;
using Store.Identity.Infrastructure.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Store.Identity.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly JwtOptions _jwtOptions;

    public UsersController(IMediator mediator, IOptions<JwtOptions> jwtOptions)
    {
        _mediator = mediator;
        _jwtOptions = jwtOptions.Value;
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
            Response.Cookies.Append("tasty-cookies", result.Value.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(_jwtOptions.ExpiresHours)
            });
        }
        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }
    
    [Authorize(Roles = "Manager")]
    [HttpGet]
    public async Task<ActionResult<GetUserDto>> GetUserByEmail([FromQuery] GetUserQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }
    
}