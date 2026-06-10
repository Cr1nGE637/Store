namespace Store.App.API.DTOs;

public sealed record ProductImportResultResponse(
    int TotalRows,
    int CreatedCount,
    int UpdatedCount,
    int StockUpdatedCount);
