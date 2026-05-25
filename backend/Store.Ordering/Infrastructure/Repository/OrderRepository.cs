using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Ordering.Domain.Aggregates;
using Store.Ordering.Domain.ValueObjects;
using Store.Ordering.Domain.Interfaces;
using Store.Ordering.Infrastructure.DbContexts;
using Store.Ordering.Infrastructure.Entity;
using System.Security.Cryptography;

namespace Store.Ordering.Infrastructure.Repository;

public class OrderRepository(OrderingDbContext context) : IOrderRepository
{
    public async Task<Result<Order>> GetByIdAsync(Guid orderId)
    {
        var entity = await context.Orders
            .AsNoTracking()
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (entity == null)
            return Result.Failure<Order>("Order not found");

        return Result.Success(MapToDomain(entity));
    }

    public async Task<Result<Order>> GetBySourceCheckoutIdAsync(Guid sourceCheckoutId)
    {
        var entity = await context.Orders
            .AsNoTracking()
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.SourceCheckoutId == sourceCheckoutId);

        if (entity == null)
            return Result.Failure<Order>("Order not found");

        return Result.Success(MapToDomain(entity));
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        var entities = await context.Orders
            .AsNoTracking()
            .Include(o => o.Products)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, int skip, int take)
    {
        var entities = await context.Orders
            .AsNoTracking()
            .Include(o => o.Products)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<Result> AddAsync(Order order)
    {
        await context.Orders.AddAsync(MapToEntity(order));
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(Order order)
    {
        var entity = await context.Orders.FindAsync(order.OrderId);
        if (entity == null)
            return Result.Failure($"Order {order.OrderId} not found for update");

        entity.Status = order.Status;
        entity.PaidAt = order.PaidAt;
        entity.PaidAmount = order.PaidAmount;
        entity.PaymentTransactionId = order.PaymentTransactionId;
        entity.CancelledAt = order.CancelledAt;
        entity.RejectedAt = order.RejectedAt;
        entity.RejectionReason = order.RejectionReason;
        return Result.Success();
    }

    private static Order MapToDomain(OrderEntity entity)
    {
        var order = Order.Reconstitute(
            entity.OrderId, entity.SourceCheckoutId, entity.CustomerId, entity.CustomerEmail,
            entity.RecipientName, entity.Phone, entity.PaymentMethod, entity.Status,
            entity.CreatedAt, entity.PaidAt, entity.CancelledAt, entity.RejectedAt,
            entity.PaidAmount, entity.PaymentTransactionId, entity.RejectionReason);
        var products = entity.Products.Select(p =>
        {
            var result = OrderedProduct.Create(p.ProductId, p.ProductName, p.Price, p.Quantity);
            if (result.IsFailure)
                throw new InvalidOperationException(
                    $"Corrupted OrderedProduct in DB (OrderId={entity.OrderId}): {result.Error}");
            return result.Value;
        });
        order.LoadProducts(products);
        return order;
    }

    private static OrderEntity MapToEntity(Order order) => new()
    {
        OrderId = order.OrderId,
        SourceCheckoutId = order.SourceCheckoutId,
        CustomerId = order.CustomerId,
        CustomerEmail = order.CustomerEmail,
        RecipientName = order.RecipientName,
        Phone = order.Phone,
        PaymentMethod = order.PaymentMethod,
        Status = order.Status,
        CreatedAt = order.CreatedAt,
        PaidAt = order.PaidAt,
        PaidAmount = order.PaidAmount,
        PaymentTransactionId = order.PaymentTransactionId,
        CancelledAt = order.CancelledAt,
        RejectedAt = order.RejectedAt,
        RejectionReason = order.RejectionReason,
        Products = order.Products.Select(p => new OrderedProductEntity
        {
            OrderedProductId = CreateOrderedProductId(order.OrderId, p.ProductId),
            OrderId = order.OrderId,
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Price = p.Price,
            Quantity = p.Quantity
        }).ToList()
    };

    private static Guid CreateOrderedProductId(Guid orderId, Guid productId)
    {
        Span<byte> source = stackalloc byte[32];
        orderId.TryWriteBytes(source[..16]);
        productId.TryWriteBytes(source[16..]);

        Span<byte> hash = stackalloc byte[16];
        MD5.HashData(source, hash);
        return new Guid(hash);
    }
}
