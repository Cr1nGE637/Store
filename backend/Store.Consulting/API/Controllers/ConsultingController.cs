using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.Consulting.API.Requests;
using Store.Consulting.Application.CQRS.Command;
using Store.Consulting.Application.CQRS.Query;
using Store.Consulting.Application.DTOs;

namespace Store.Consulting.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public sealed class ConsultingController(IMediator mediator) : ControllerBase
{
    [HttpPost("cart/check")]
    [ProducesResponseType(typeof(ConsultationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckCartCompatibility(
        [FromBody] CheckCartCompatibilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CheckCartCompatibilityQuery
        {
            CustomerId = TryGetCustomerId(out var customerId) ? customerId : null,
            ProductIds = request.ProductIds ?? []
        }, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    [AllowAnonymous]
    [HttpGet("products/{productId:guid}/recommendations")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductRecommendations(
        Guid productId,
        [FromQuery] int limit,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductRecommendationsQuery
        {
            ProductId = productId,
            Limit = limit <= 0 ? 6 : limit
        }, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("rules")]
    [ProducesResponseType(typeof(IReadOnlyCollection<CompatibilityRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRules(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCompatibilityRulesQuery(), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    [Authorize(Roles = "Manager")]
    [HttpPost("rules")]
    [ProducesResponseType(typeof(CompatibilityRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRule(
        [FromBody] CompatibilityRulePayloadRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateCompatibilityRuleCommand
        {
            Payload = ToPayload(request)
        }, cancellationToken);

        return result.IsSuccess
            ? Created(string.Empty, result.Value)
            : Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    [Authorize(Roles = "Manager")]
    [HttpPut("rules/{ruleId:guid}")]
    [ProducesResponseType(typeof(CompatibilityRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRule(
        Guid ruleId,
        [FromBody] CompatibilityRulePayloadRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateCompatibilityRuleCommand
        {
            RuleId = ruleId,
            Payload = ToPayload(request)
        }, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    [Authorize(Roles = "Manager")]
    [HttpDelete("rules/{ruleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteRule(Guid ruleId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteCompatibilityRuleCommand { RuleId = ruleId }, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    [Authorize(Roles = "Manager")]
    [HttpPost("rules/test")]
    [ProducesResponseType(typeof(ConsultationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TestRule(
        [FromBody] TestCompatibilityRuleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new TestCompatibilityRuleQuery
        {
            SourceProductId = request.SourceProductId,
            TargetProductId = request.TargetProductId,
            Payload = ToPayload(request.Rule)
        }, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    private bool TryGetCustomerId(out Guid customerId)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out customerId);
    }

    private static CompatibilityRulePayloadDto ToPayload(CompatibilityRulePayloadRequest request) =>
        new(
            request.Code,
            request.Name,
            request.SourceCategoryCode,
            request.TargetCategoryCode,
            request.SourceSpecificationKey,
            request.TargetSpecificationKey,
            request.Operator,
            request.ExpectedValue,
            request.Severity,
            request.MessageTemplate,
            request.RecommendationType,
            request.IsActive);
}
