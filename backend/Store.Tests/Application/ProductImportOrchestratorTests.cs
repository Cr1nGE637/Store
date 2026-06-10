using CSharpFunctionalExtensions;
using MediatR;
using Store.App.Application;
using Store.Catalog.Application.CQRS.Command;
using Store.Catalog.Application.DTOs;
using Store.Inventory.Contracts;

namespace Store.Tests.Application;

public class ProductImportOrchestratorTests
{
    [Fact]
    public async Task ImportAsync_WhenCatalogImportSucceeds_UpdatesInventoryThroughContract()
    {
        var productId = Guid.NewGuid();
        var mediator = new FakeMediator(new ProductImportResultDto(
            TotalRows: 1,
            CreatedCount: 1,
            UpdatedCount: 0,
            StockUpdates: [new ProductImportStockUpdateDto(productId, 7)]));
        var stockWriter = new FakeInventoryStockWriter();
        var orchestrator = new ProductImportOrchestrator(mediator, stockWriter);

        await using var stream = new MemoryStream([1, 2, 3]);
        var result = await orchestrator.ImportAsync(stream, "products.xlsx", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.CreatedCount);
        Assert.Equal(1, result.Value.StockUpdatedCount);
        Assert.Contains(stockWriter.Updates, update => update.ProductId == productId && update.AvailableQuantity == 7);
    }

    private sealed class FakeMediator(ProductImportResultDto result) : IMediator
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Assert.IsType<ImportProductsCommand>(request);
            return Task.FromResult((TResponse)(object)Result.Success(result));
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest =>
            Task.CompletedTask;

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            Task.FromResult<object?>(Result.Success(result));

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default) =>
            EmptyStream<TResponse>();

        public IAsyncEnumerable<object?> CreateStream(
            object request,
            CancellationToken cancellationToken = default) =>
            EmptyStream<object?>();

        public Task Publish(object notification, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification =>
            Task.CompletedTask;

        private static async IAsyncEnumerable<T> EmptyStream<T>()
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    private sealed class FakeInventoryStockWriter : IInventoryStockWriter
    {
        public List<(Guid ProductId, int AvailableQuantity)> Updates { get; } = [];

        public Task<InventoryStockWriteResult> SetAvailableQuantityAsync(
            Guid productId,
            int availableQuantity,
            CancellationToken cancellationToken)
        {
            Updates.Add((productId, availableQuantity));
            return Task.FromResult(InventoryStockWriteResult.Success());
        }
    }
}
