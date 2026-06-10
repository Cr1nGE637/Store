namespace Store.Consulting.API.Requests;

public sealed class CompatibilityRulePayloadRequest
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string SourceCategoryCode { get; init; } = string.Empty;
    public string TargetCategoryCode { get; init; } = string.Empty;
    public string SourceSpecificationKey { get; init; } = string.Empty;
    public string TargetSpecificationKey { get; init; } = string.Empty;
    public string Operator { get; init; } = string.Empty;
    public string? ExpectedValue { get; init; }
    public string Severity { get; init; } = string.Empty;
    public string MessageTemplate { get; init; } = string.Empty;
    public string? RecommendationType { get; init; }
    public bool IsActive { get; init; } = true;
}
