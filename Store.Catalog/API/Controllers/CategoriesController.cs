using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<CreateCategoryDto>> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken token)
    {
        var result = await _mediator.Send(command, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Created(string.Empty, result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<List<GetCategoryDto>>> GetAllCategories([FromQuery] GetCategoriesQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetCategoryDto>> GetCategoryById(Guid id, CancellationToken token)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id), token);
        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GetCategoryDto>> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command, CancellationToken token)
    {
        var commandWithId = new UpdateCategoryCommand { CategoryId = id, CategoryName = command.CategoryName };
        var result = await _mediator.Send(commandWithId, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [Authorize(Roles = "Manager")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<GetCategoryDto>> DeleteCategory(Guid id, CancellationToken token)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand { CategoryId = id }, token);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
