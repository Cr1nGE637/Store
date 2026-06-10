namespace Store.Consulting.Infrastructure.Entity;

public sealed class RecommendationEventEntity
{
    public Guid Id { get; set; }
    public Guid ConsultationId { get; set; }
    public Guid ProductId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
