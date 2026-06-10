namespace Store.Consulting.Application.DTOs;

public sealed record ConsultationItemDto(
    Guid ProductId,
    string CategoryCode,
    IReadOnlyDictionary<string, string> Specifications);
