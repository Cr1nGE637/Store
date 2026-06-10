using Store.Consulting.Domain.Enums;

namespace Store.Consulting.Domain.Entities;

public sealed record ProductRecommendation(
    RecommendationType Type,
    string Reason,
    Guid? SourceProductId,
    Guid? TargetProductId);
