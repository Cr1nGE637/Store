using Store.Consulting.Domain.Enums;

namespace Store.Consulting.Domain.Entities;

public sealed record ConsultationResult(
    ConsultationStatus Status,
    IReadOnlyCollection<CompatibilityIssue> Issues,
    IReadOnlyCollection<ProductRecommendation> Recommendations,
    DateTime CheckedAtUtc);
