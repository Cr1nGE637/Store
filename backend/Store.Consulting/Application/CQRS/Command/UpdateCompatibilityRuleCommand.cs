using CSharpFunctionalExtensions;
using MediatR;
using Store.Consulting.Application.DTOs;

namespace Store.Consulting.Application.CQRS.Command;

public sealed class UpdateCompatibilityRuleCommand : IRequest<Result<CompatibilityRuleDto>>
{
    public Guid RuleId { get; init; }
    public CompatibilityRulePayloadDto Payload { get; init; } = new(
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        null,
        string.Empty,
        string.Empty,
        null,
        true);
}
