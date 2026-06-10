namespace Store.Catalog.Application.DTOs;

public sealed record ProductImportPreviewDto(
    int TotalRows,
    int CreateCount,
    int UpdateCount,
    int ErrorCount,
    IReadOnlyCollection<ProductImportPreviewRowDto> Rows,
    IReadOnlyCollection<string> Errors);

public sealed record ProductImportPreviewRowDto(
    int RowNumber,
    string Sku,
    string? Name,
    string? CategoryCode,
    decimal? Price,
    int? StockQuantity,
    string Action,
    IReadOnlyCollection<string> Errors);
