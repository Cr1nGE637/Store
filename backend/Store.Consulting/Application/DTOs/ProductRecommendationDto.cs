namespace Store.Consulting.Application.DTOs;

public sealed record ProductRecommendationDto(
    Guid ProductId,
    string Type,
    string Reason);
