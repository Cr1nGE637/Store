using System.Text.Json.Serialization;

namespace Store.Catalog.Application.DTOs;

public sealed record ProductImageDto(
    Guid ProductImageId,
    Guid ProductId,
    string Url,
    [property: JsonIgnore] string StoragePath,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string AltText,
    bool IsMain,
    int DisplayOrder,
    DateTime CreatedAtUtc);
