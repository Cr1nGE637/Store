using Store.Identity.Application.CQRS.Command;
using Store.Identity.Application.CQRS.Query;
using Store.Identity.Application.DTOs;
using Store.Identity.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Store.Identity.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAuthCookieSettings _authCookieSettings;

    public UsersController(IMediator mediator, IAuthCookieSettings authCookieSettings)
    {
        _mediator = mediator;
        _authCookieSettings = authCookieSettings;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterDto>> Register([FromBody] RegisterCommand command, CancellationToken token)
    {
        var result = await _mediator.Send(command, token);

        if (result.IsFailure)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(LoginDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginDto>> Login([FromBody] LoginCommand command, CancellationToken token)
    {
        var result = await _mediator.Send(command, token);
        if (result.IsSuccess)
        {
            Response.Cookies.Append(_authCookieSettings.CookieName, result.Value.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = _authCookieSettings.ExpiresAtUtc()
            });
        }
        if (result.IsFailure)
            return Unauthorized(result.Error);
        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(_authCookieSettings.CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict
        });

        return NoContent();
    }
    
    [Authorize(Roles = "Manager")]
    [HttpGet]
    [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetUserDto>> GetUserByEmail([FromQuery] GetUserQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }
    
}
