using CSharpFunctionalExtensions;
using MediatR;
using Store.App.API.DTOs;
using Store.Catalog.Application.CQRS.Command;
using Store.Inventory.Contracts;

namespace Store.App.Application;

public sealed class ProductImportOrchestrator(
    IMediator mediator,
    IInventoryStockWriter stockWriter)
{
    public async Task<Result<ProductImportResultResponse>> ImportAsync(
        Stream content,
        string fileName,
        CancellationToken cancellationToken)
    {
        var catalogResult = await mediator.Send(new ImportProductsCommand(content, fileName), cancellationToken);
        if (catalogResult.IsFailure)
            return Result.Failure<ProductImportResultResponse>(catalogResult.Error);

        var stockUpdatedCount = 0;
        foreach (var stockUpdate in catalogResult.Value.StockUpdates)
        {
            var stockResult = await stockWriter.SetAvailableQuantityAsync(
                stockUpdate.ProductId,
                stockUpdate.AvailableQuantity,
                cancellationToken);
            if (!stockResult.IsSuccess)
                return Result.Failure<ProductImportResultResponse>(stockResult.Error);

            stockUpdatedCount++;
        }

        return Result.Success(new ProductImportResultResponse(
            catalogResult.Value.TotalRows,
            catalogResult.Value.CreatedCount,
            catalogResult.Value.UpdatedCount,
            stockUpdatedCount));
    }
}
