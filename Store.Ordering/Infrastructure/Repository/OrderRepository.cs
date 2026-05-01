using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Ordering.Domain.Aggregates;
using Store.Ordering.Domain.ValueObjects;
using Store.Ordering.Domain.Interfaces;
using Store.Ordering.Infrastructure.DbContexts;
using Store.Ordering.Infrastructure.Entities;

namespace Store.Ordering.Infrastructure.Repository;

public class OrderRepository(OrderingDbContext context) : IOrderRepository
{
    public async Task<Result<Order>> GetByIdAsync(Guid orderId)
    {
        var entity = await context.Orders
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (entity == null)
            return Result.Failure<Order>("Order not found");

        return Result.Success(MapToDomain(entity));
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        var entities = await context.Orders
            .Include(o => o.Products)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();

        return entities.Select(MapToDomain).ToList();
    }

    public async Task AddAsync(Order order)
    {
        await context.Orders.AddAsync(MapToEntity(order));
    }

    public async Task UpdateAsync(Order order)
    {
        var entity = await context.Orders.FindAsync(order.OrderId);
        if (entity != null)
        {
            entity.Status = order.Status;
            entity.PaidAt = order.PaidAt;
            entity.CancelledAt = order.CancelledAt;
        }
    }

    private static Order MapToDomain(OrderEntity entity)
    {
        var order = Order.Reconstitute(
            entity.OrderId, entity.CustomerId, entity.Status,
            entity.CreatedAt, entity.PaidAt, entity.CancelledAt);
        var products = entity.Products.Select(p =>
            OrderedProduct.Create(p.ProductId, p.ProductName, p.Price, p.Quantity).Value);
        order.LoadProducts(products);
        return order;
    }

    private static OrderEntity MapToEntity(Order order) => new()
    {
        OrderId = order.OrderId,
        CustomerId = order.CustomerId,
        Status = order.Status,
        CreatedAt = order.CreatedAt,
        PaidAt = order.PaidAt,
        CancelledAt = order.CancelledAt,
        Products = order.Products.Select(p => new OrderedProductEntity
        {
            OrderedProductId = Guid.NewGuid(),
            OrderId = order.OrderId,
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Price = p.Price,
            Quantity = p.Quantity
        }).ToList()
    };
}
