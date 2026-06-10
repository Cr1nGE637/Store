using CSharpFunctionalExtensions;
using Store.Consulting.Domain.Entities;

namespace Store.Consulting.Application.Interfaces;

public interface ICompatibilityRuleProvider
{
    Task<Result<IReadOnlyCollection<CompatibilityRule>>> GetActiveRulesAsync(CancellationToken cancellationToken);
}
