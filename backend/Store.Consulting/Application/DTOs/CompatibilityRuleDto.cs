namespace Store.Consulting.Application.DTOs;

public sealed record CompatibilityRuleDto(
    Guid Id,
    string Code,
    string Name,
    string SourceCategoryCode,
    string TargetCategoryCode,
    string SourceSpecificationKey,
    string TargetSpecificationKey,
    string Operator,
    string? ExpectedValue,
    string Severity,
    string MessageTemplate,
    string? RecommendationType,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
