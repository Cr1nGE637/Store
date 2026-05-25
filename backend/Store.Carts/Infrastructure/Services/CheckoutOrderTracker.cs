using Microsoft.EntityFrameworkCore;
using Store.Carts.Application.Interfaces;
using Store.Carts.Infrastructure.DbContexts;
using Store.Carts.Infrastructure.Entity;

namespace Store.Carts.Infrastructure.Services;

public class CheckoutOrderTracker(CartDbContext dbContext) : ICheckoutOrderLookup, ICheckoutOrderRecorder
{
    public Task<bool> HasOrderForCheckoutAsync(Guid checkoutId, CancellationToken cancellationToken) =>
        dbContext.CheckoutOrders.AnyAsync(o => o.CheckoutId == checkoutId, cancellationToken);

    public async Task RecordOrderCreatedAsync(
        Guid checkoutId,
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.CheckoutOrders
            .AnyAsync(o => o.CheckoutId == checkoutId, cancellationToken);
        if (exists)
            return;

        dbContext.CheckoutOrders.Add(new CheckoutOrderEntity
        {
            CheckoutId = checkoutId,
            OrderId = orderId,
            CustomerId = customerId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
    }
}
