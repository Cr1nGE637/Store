using CSharpFunctionalExtensions;
using Store.Consulting.Application.DTOs;
using Store.Consulting.Domain.Entities;

namespace Store.Consulting.Application.Interfaces;

public interface ICompatibilityRuleRepository
{
    Task<Result<IReadOnlyCollection<CompatibilityRuleDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<CompatibilityRuleDto>> GetByIdAsync(Guid ruleId, CancellationToken cancellationToken);
    Task<Result<CompatibilityRuleDto>> AddAsync(CompatibilityRule rule, CancellationToken cancellationToken);
    Task<Result<CompatibilityRuleDto>> UpdateAsync(Guid ruleId, CompatibilityRule rule, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid ruleId, CancellationToken cancellationToken);
}
