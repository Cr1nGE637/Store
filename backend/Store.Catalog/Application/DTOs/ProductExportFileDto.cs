namespace Store.Catalog.Application.DTOs;

public sealed record ProductExportFileDto(
    string FileName,
    string ContentType,
    byte[] Content);
