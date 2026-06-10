using Store.Consulting.Domain.Enums;

namespace Store.Consulting.Domain.Entities;

public sealed record CompatibilityIssue(
    CompatibilitySeverity Severity,
    string RuleCode,
    string Message,
    Guid? SourceProductId,
    Guid? TargetProductId);
