namespace Store.Catalog.Application.DTOs;

public sealed record ProductImportResultDto(
    int TotalRows,
    int CreatedCount,
    int UpdatedCount,
    IReadOnlyCollection<ProductImportStockUpdateDto> StockUpdates)
{
    public int StockUpdatedCount => StockUpdates.Count;
}

public sealed record ProductImportStockUpdateDto(Guid ProductId, int AvailableQuantity);
