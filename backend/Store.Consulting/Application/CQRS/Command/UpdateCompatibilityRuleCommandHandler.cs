using CSharpFunctionalExtensions;
using MediatR;
using Store.Consulting.Application.DTOs;
using Store.Consulting.Application.Interfaces;

namespace Store.Consulting.Application.CQRS.Command;

public sealed class UpdateCompatibilityRuleCommandHandler(ICompatibilityRuleRepository repository)
    : IRequestHandler<UpdateCompatibilityRuleCommand, Result<CompatibilityRuleDto>>
{
    public async Task<Result<CompatibilityRuleDto>> Handle(
        UpdateCompatibilityRuleCommand request,
        CancellationToken cancellationToken)
    {
        if (request.RuleId == Guid.Empty)
            return Result.Failure<CompatibilityRuleDto>("Compatibility rule id is required");

        var ruleResult = CompatibilityRulePayloadMapper.ToDomain(request.Payload);
        if (ruleResult.IsFailure)
            return Result.Failure<CompatibilityRuleDto>(ruleResult.Error);

        return await repository.UpdateAsync(request.RuleId, ruleResult.Value, cancellationToken);
    }
}
