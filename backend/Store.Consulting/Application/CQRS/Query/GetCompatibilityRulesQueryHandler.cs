using CSharpFunctionalExtensions;
using MediatR;
using Store.Consulting.Application.DTOs;
using Store.Consulting.Application.Interfaces;

namespace Store.Consulting.Application.CQRS.Query;

public sealed class GetCompatibilityRulesQueryHandler(ICompatibilityRuleRepository repository)
    : IRequestHandler<GetCompatibilityRulesQuery, Result<IReadOnlyCollection<CompatibilityRuleDto>>>
{
    public Task<Result<IReadOnlyCollection<CompatibilityRuleDto>>> Handle(
        GetCompatibilityRulesQuery request,
        CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);
}
