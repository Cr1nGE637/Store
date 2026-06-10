using CSharpFunctionalExtensions;
using MediatR;
using Store.Consulting.Application.DTOs;
using Store.Consulting.Application.Interfaces;

namespace Store.Consulting.Application.CQRS.Command;

public sealed class CreateCompatibilityRuleCommandHandler(ICompatibilityRuleRepository repository)
    : IRequestHandler<CreateCompatibilityRuleCommand, Result<CompatibilityRuleDto>>
{
    public async Task<Result<CompatibilityRuleDto>> Handle(
        CreateCompatibilityRuleCommand request,
        CancellationToken cancellationToken)
    {
        var ruleResult = CompatibilityRulePayloadMapper.ToDomain(request.Payload);
        if (ruleResult.IsFailure)
            return Result.Failure<CompatibilityRuleDto>(ruleResult.Error);

        return await repository.AddAsync(ruleResult.Value, cancellationToken);
    }
}
