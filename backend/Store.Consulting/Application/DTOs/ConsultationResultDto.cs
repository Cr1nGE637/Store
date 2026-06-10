namespace Store.Consulting.Application.DTOs;

public sealed record ConsultationResultDto(
    Guid ConsultationId,
    string Status,
    IReadOnlyCollection<ConsultationItemDto> Items,
    IReadOnlyCollection<CompatibilityFindingDto> Findings,
    IReadOnlyCollection<ProductRecommendationDto> Recommendations,
    DateTime CheckedAtUtc);
