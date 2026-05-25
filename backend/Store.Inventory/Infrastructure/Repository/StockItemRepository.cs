using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Inventory.Domain.Aggregates;
using Store.Inventory.Domain.Entities;
using Store.Inventory.Domain.Interfaces;
using Store.Inventory.Infrastructure.DbContexts;
using Store.Inventory.Infrastructure.Entity;

namespace Store.Inventory.Infrastructure.Repository;

public class StockItemRepository(InventoryDbContext context) : IStockItemRepository
{
    public async Task<Result<StockItem>> GetByProductIdAsync(Guid productId)
    {
        var entity = await context.StockItems
            .Include(s => s.Reservations)
            .FirstOrDefaultAsync(s => s.ProductId == productId);

        if (entity == null)
            return Result.Failure<StockItem>($"Stock not found for product {productId}");

        return Result.Success(ToDomain(entity));
    }

    public async Task<Result> AddAsync(StockItem stockItem)
    {
        await context.StockItems.AddAsync(ToEntity(stockItem));
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(StockItem stockItem)
    {
        var entity = await context.StockItems
            .Include(s => s.Reservations)
            .FirstOrDefaultAsync(s => s.Id == stockItem.Id);
        if (entity == null)
            return Result.Failure($"StockItem {stockItem.Id} not found for update");

        entity.Quantity = stockItem.Quantity;
        entity.Reserved = stockItem.Reserved;
        SyncReservations(entity, stockItem.Reservations);
        return Result.Success();
    }

    private static StockItem ToDomain(StockItemEntity entity) =>
        StockItem.Reconstitute(
            entity.Id,
            entity.ProductId,
            entity.Quantity,
            entity.Reserved,
            entity.Reservations.Select(ToDomainReservation));

    private static StockItemEntity ToEntity(StockItem stockItem) => new()
    {
        Id = stockItem.Id,
        ProductId = stockItem.ProductId,
        Quantity = stockItem.Quantity,
        Reserved = stockItem.Reserved,
        Reservations = stockItem.Reservations.Select(reservation => ToEntityReservation(stockItem.Id, reservation)).ToList()
    };

    private static StockReservation ToDomainReservation(StockReservationEntity entity) =>
        StockReservation.Reconstitute(entity.Id, entity.OrderId, entity.Quantity);

    private static StockReservationEntity ToEntityReservation(Guid stockItemId, StockReservation reservation) => new()
    {
        Id = reservation.Id,
        StockItemId = stockItemId,
        OrderId = reservation.OrderId,
        Quantity = reservation.Quantity
    };

    private static void SyncReservations(StockItemEntity entity, IReadOnlyCollection<StockReservation> reservations)
    {
        var reservationIds = reservations.Select(r => r.Id).ToHashSet();
        entity.Reservations.RemoveAll(r => !reservationIds.Contains(r.Id));

        foreach (var reservation in reservations)
        {
            var reservationEntity = entity.Reservations.SingleOrDefault(r => r.Id == reservation.Id);
            if (reservationEntity is null)
            {
                entity.Reservations.Add(ToEntityReservation(entity.Id, reservation));
                continue;
            }

            reservationEntity.OrderId = reservation.OrderId;
            reservationEntity.Quantity = reservation.Quantity;
        }
    }
}
