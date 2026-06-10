using CSharpFunctionalExtensions;
using MediatR;

namespace Store.Consulting.Application.CQRS.Command;

public sealed class DeleteCompatibilityRuleCommand : IRequest<Result>
{
    public Guid RuleId { get; init; }
}
