namespace Store.Consulting.Infrastructure.Entity;

public sealed class ConsultationResultEntity
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ProductIdsJson { get; set; } = "[]";
    public string ItemsJson { get; set; } = "[]";
    public string FindingsJson { get; set; } = "[]";
    public string RecommendationsJson { get; set; } = "[]";
    public DateTime CheckedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
