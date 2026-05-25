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
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Manager")]
    [HttpPost]
    [ProducesResponseType(typeof(CreateCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateCategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken token)
    {
        var command = new CreateCategoryCommand
        {
            CategoryName = request.CategoryName,
            CategoryCode = request.CategoryCode
        };

        var result = await _mediator.Send(command, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Created(string.Empty, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GetCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<GetCategoryDto>>> GetAllCategories([FromQuery] GetCategoriesQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetCategoryDto>> GetCategoryById(Guid id, CancellationToken token)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id), token);
        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GetCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetCategoryDto>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken token)
    {
        var commandWithId = new UpdateCategoryCommand
        {
            CategoryId = id,
            CategoryName = request.CategoryName,
            CategoryCode = request.CategoryCode
        };

        var result = await _mediator.Send(commandWithId, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(GetCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetCategoryDto>> DeleteCategory(Guid id, CancellationToken token)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand { CategoryId = id }, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
