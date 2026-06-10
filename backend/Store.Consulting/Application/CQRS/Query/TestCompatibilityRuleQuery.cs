using CSharpFunctionalExtensions;
using MediatR;
using Store.Consulting.Application.DTOs;

namespace Store.Consulting.Application.CQRS.Query;

public sealed class TestCompatibilityRuleQuery : IRequest<Result<ConsultationResultDto>>
{
    public Guid SourceProductId { get; init; }
    public Guid TargetProductId { get; init; }
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
