using CSharpFunctionalExtensions;
using Store.Consulting.Domain.Enums;

namespace Store.Consulting.Domain.Entities;

public sealed class CompatibilityRule
{
    private CompatibilityRule(
        Guid id,
        string code,
        string name,
        string sourceCategoryCode,
        string targetCategoryCode,
        CompatibilityCondition condition,
        CompatibilitySeverity severity,
        string messageTemplate,
        RecommendationType? recommendationType,
        bool isActive)
    {
        Id = id;
        Code = code;
        Name = name;
        SourceCategoryCode = sourceCategoryCode;
        TargetCategoryCode = targetCategoryCode;
        Condition = condition;
        Severity = severity;
        MessageTemplate = messageTemplate;
        RecommendationType = recommendationType;
        IsActive = isActive;
    }

    public Guid Id { get; }
    public string Code { get; }
    public string Name { get; }
    public string SourceCategoryCode { get; }
    public string TargetCategoryCode { get; }
    public CompatibilityCondition Condition { get; }
    public CompatibilitySeverity Severity { get; }
    public string MessageTemplate { get; }
    public RecommendationType? RecommendationType { get; }
    public bool IsActive { get; }

    public static Result<CompatibilityRule> Create(
        string code,
        string name,
        string sourceCategoryCode,
        string targetCategoryCode,
        CompatibilityCondition condition,
        CompatibilitySeverity severity,
        string messageTemplate,
        RecommendationType? recommendationType = null,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<CompatibilityRule>("Compatibility rule code is required");

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<CompatibilityRule>("Compatibility rule name is required");

        if (string.IsNullOrWhiteSpace(sourceCategoryCode))
            return Result.Failure<CompatibilityRule>("Source category code is required");

        if (string.IsNullOrWhiteSpace(targetCategoryCode))
            return Result.Failure<CompatibilityRule>("Target category code is required");

        if (string.IsNullOrWhiteSpace(messageTemplate))
            return Result.Failure<CompatibilityRule>("Compatibility rule message is required");

        return Result.Success(new CompatibilityRule(
            Guid.NewGuid(),
            code.Trim(),
            name.Trim(),
            sourceCategoryCode.Trim(),
            targetCategoryCode.Trim(),
            condition,
            severity,
            messageTemplate.Trim(),
            recommendationType,
            isActive));
    }

    public static Result<CompatibilityRule> Reconstitute(
        Guid id,
        string code,
        string name,
        string sourceCategoryCode,
        string targetCategoryCode,
        CompatibilityCondition condition,
        CompatibilitySeverity severity,
        string messageTemplate,
        RecommendationType? recommendationType,
        bool isActive)
    {
        if (id == Guid.Empty)
            return Result.Failure<CompatibilityRule>("Compatibility rule id is required");

        var result = Create(
            code,
            name,
            sourceCategoryCode,
            targetCategoryCode,
            condition,
            severity,
            messageTemplate,
            recommendationType,
            isActive);

        if (result.IsFailure)
            return result;

        return Result.Success(new CompatibilityRule(
            id,
            result.Value.Code,
            result.Value.Name,
            result.Value.SourceCategoryCode,
            result.Value.TargetCategoryCode,
            result.Value.Condition,
            result.Value.Severity,
            result.Value.MessageTemplate,
            result.Value.RecommendationType,
            result.Value.IsActive));
    }
}
