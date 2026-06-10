namespace Store.Consulting.API.Requests;

public sealed class CheckCartCompatibilityRequest
{
    public IReadOnlyCollection<Guid> ProductIds { get; init; } = [];
}
