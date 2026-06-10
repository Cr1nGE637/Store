namespace Store.Consulting.Infrastructure.Entity;

public sealed class CompatibilityRuleEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SourceCategoryCode { get; set; } = string.Empty;
    public string TargetCategoryCode { get; set; } = string.Empty;
    public string SourceSpecificationKey { get; set; } = string.Empty;
    public string TargetSpecificationKey { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public string? RecommendationType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
