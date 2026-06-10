namespace Store.Consulting.API.Requests;

public sealed class TestCompatibilityRuleRequest
{
    public Guid SourceProductId { get; init; }
    public Guid TargetProductId { get; init; }
    public CompatibilityRulePayloadRequest Rule { get; init; } = new();
}
