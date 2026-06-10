namespace Store.Consulting.Application.DTOs;

public sealed record CompatibilityFindingDto(
    string Severity,
    string RuleCode,
    string Message);
