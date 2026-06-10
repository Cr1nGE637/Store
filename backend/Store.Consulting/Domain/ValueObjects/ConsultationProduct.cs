namespace Store.Consulting.Domain.ValueObjects;

public sealed record ConsultationProduct(
    Guid ProductId,
    string ProductName,
    string CategoryCode,
    IReadOnlyDictionary<string, string> Specifications);
