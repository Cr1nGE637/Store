using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Consulting.Application.DTOs;
using Store.Consulting.Application.Interfaces;
using Store.Consulting.Domain.Entities;
using Store.Consulting.Domain.Enums;
using Store.Consulting.Infrastructure.DbContexts;
using Store.Consulting.Infrastructure.Entity;

namespace Store.Consulting.Infrastructure.Repository;

public sealed class CompatibilityRuleRepository(ConsultingDbContext dbContext)
    : ICompatibilityRuleRepository, ICompatibilityRuleProvider
{
    public async Task<Result<IReadOnlyCollection<CompatibilityRule>>> GetActiveRulesAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.CompatibilityRules
            .AsNoTracking()
            .Where(rule => rule.IsActive)
            .OrderBy(rule => rule.Code)
            .ToListAsync(cancellationToken);

        var rules = new List<CompatibilityRule>();
        foreach (var entity in entities)
        {
            var result = ToDomain(entity);
            if (result.IsFailure)
                return Result.Failure<IReadOnlyCollection<CompatibilityRule>>(result.Error);

            rules.Add(result.Value);
        }

        return Result.Success<IReadOnlyCollection<CompatibilityRule>>(rules);
    }

    public async Task<Result<IReadOnlyCollection<CompatibilityRuleDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var rules = await dbContext.CompatibilityRules
            .AsNoTracking()
            .OrderBy(rule => rule.SourceCategoryCode)
            .ThenBy(rule => rule.TargetCategoryCode)
            .ThenBy(rule => rule.Code)
            .Select(rule => ToDto(rule))
            .ToArrayAsync(cancellationToken);

        return Result.Success<IReadOnlyCollection<CompatibilityRuleDto>>(rules);
    }

    public async Task<Result<CompatibilityRuleDto>> GetByIdAsync(Guid ruleId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.CompatibilityRules
            .AsNoTracking()
            .FirstOrDefaultAsync(rule => rule.Id == ruleId, cancellationToken);

        return entity is null
            ? Result.Failure<CompatibilityRuleDto>("Compatibility rule not found")
            : Result.Success(ToDto(entity));
    }

    public async Task<Result<CompatibilityRuleDto>> AddAsync(CompatibilityRule rule, CancellationToken cancellationToken)
    {
        if (await dbContext.CompatibilityRules.AnyAsync(existing => existing.Code == rule.Code, cancellationToken))
            return Result.Failure<CompatibilityRuleDto>("Compatibility rule code already exists");

        var entity = ToEntity(rule);
        entity.CreatedAtUtc = DateTime.UtcNow;
        await dbContext.CompatibilityRules.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(entity));
    }

    public async Task<Result<CompatibilityRuleDto>> UpdateAsync(
        Guid ruleId,
        CompatibilityRule rule,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.CompatibilityRules
            .FirstOrDefaultAsync(existing => existing.Id == ruleId, cancellationToken);
        if (entity is null)
            return Result.Failure<CompatibilityRuleDto>("Compatibility rule not found");

        if (await dbContext.CompatibilityRules.AnyAsync(existing => existing.Id != ruleId && existing.Code == rule.Code, cancellationToken))
            return Result.Failure<CompatibilityRuleDto>("Compatibility rule code already exists");

        entity.Code = rule.Code;
        entity.Name = rule.Name;
        entity.SourceCategoryCode = rule.SourceCategoryCode;
        entity.TargetCategoryCode = rule.TargetCategoryCode;
        entity.SourceSpecificationKey = rule.Condition.SourceSpecificationKey;
        entity.TargetSpecificationKey = rule.Condition.TargetSpecificationKey;
        entity.Operator = rule.Condition.Operator.ToString();
        entity.ExpectedValue = rule.Condition.ExpectedValue;
        entity.Severity = rule.Severity.ToString();
        entity.MessageTemplate = rule.MessageTemplate;
        entity.RecommendationType = rule.RecommendationType?.ToString();
        entity.IsActive = rule.IsActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(entity));
    }

    public async Task<Result> DeleteAsync(Guid ruleId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.CompatibilityRules
            .FirstOrDefaultAsync(rule => rule.Id == ruleId, cancellationToken);
        if (entity is null)
            return Result.Failure("Compatibility rule not found");

        dbContext.CompatibilityRules.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static CompatibilityRuleEntity ToEntity(CompatibilityRule rule) => new()
    {
        Id = rule.Id,
        Code = rule.Code,
        Name = rule.Name,
        SourceCategoryCode = rule.SourceCategoryCode,
        TargetCategoryCode = rule.TargetCategoryCode,
        SourceSpecificationKey = rule.Condition.SourceSpecificationKey,
        TargetSpecificationKey = rule.Condition.TargetSpecificationKey,
        Operator = rule.Condition.Operator.ToString(),
        ExpectedValue = rule.Condition.ExpectedValue,
        Severity = rule.Severity.ToString(),
        MessageTemplate = rule.MessageTemplate,
        RecommendationType = rule.RecommendationType?.ToString(),
        IsActive = rule.IsActive
    };

    private static Result<CompatibilityRule> ToDomain(CompatibilityRuleEntity entity)
    {
        if (!Enum.TryParse<CompatibilityOperator>(entity.Operator, true, out var op))
            return Result.Failure<CompatibilityRule>("Compatibility rule operator is invalid");

        if (!Enum.TryParse<CompatibilitySeverity>(entity.Severity, true, out var severity))
            return Result.Failure<CompatibilityRule>("Compatibility rule severity is invalid");

        RecommendationType? recommendationType = null;
        if (!string.IsNullOrWhiteSpace(entity.RecommendationType))
        {
            if (!Enum.TryParse<RecommendationType>(entity.RecommendationType, true, out var parsedRecommendationType))
                return Result.Failure<CompatibilityRule>("Compatibility rule recommendation type is invalid");

            recommendationType = parsedRecommendationType;
        }

        return CompatibilityRule.Reconstitute(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.SourceCategoryCode,
            entity.TargetCategoryCode,
            new CompatibilityCondition(
                entity.SourceSpecificationKey,
                entity.TargetSpecificationKey,
                op,
                entity.ExpectedValue),
            severity,
            entity.MessageTemplate,
            recommendationType,
            entity.IsActive);
    }

    private static CompatibilityRuleDto ToDto(CompatibilityRuleEntity rule) =>
        new(
            rule.Id,
            rule.Code,
            rule.Name,
            rule.SourceCategoryCode,
            rule.TargetCategoryCode,
            rule.SourceSpecificationKey,
            rule.TargetSpecificationKey,
            rule.Operator,
            rule.ExpectedValue,
            rule.Severity,
            rule.MessageTemplate,
            rule.RecommendationType,
            rule.IsActive,
            rule.CreatedAtUtc,
            rule.UpdatedAtUtc);
}
