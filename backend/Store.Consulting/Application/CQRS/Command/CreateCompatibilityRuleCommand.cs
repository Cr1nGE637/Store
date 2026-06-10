using CSharpFunctionalExtensions;
using MediatR;
using Store.Consulting.Application.DTOs;

namespace Store.Consulting.Application.CQRS.Command;

public sealed class CreateCompatibilityRuleCommand : IRequest<Result<CompatibilityRuleDto>>
{
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
