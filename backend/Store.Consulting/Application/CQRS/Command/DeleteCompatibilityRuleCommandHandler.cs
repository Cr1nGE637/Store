using CSharpFunctionalExtensions;
using MediatR;
using Store.Consulting.Application.Interfaces;

namespace Store.Consulting.Application.CQRS.Command;

public sealed class DeleteCompatibilityRuleCommandHandler(ICompatibilityRuleRepository repository)
    : IRequestHandler<DeleteCompatibilityRuleCommand, Result>
{
    public Task<Result> Handle(DeleteCompatibilityRuleCommand request, CancellationToken cancellationToken)
    {
        if (request.RuleId == Guid.Empty)
            return Task.FromResult(Result.Failure("Compatibility rule id is required"));

        return repository.DeleteAsync(request.RuleId, cancellationToken);
    }
}
