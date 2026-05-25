using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Carts.Domain.Aggregates;
using Store.Carts.Domain.Entities;
using Store.Carts.Domain.Interfaces;
using Store.Carts.Infrastructure.DbContexts;
using Store.Carts.Infrastructure.Entity;

namespace Store.Carts.Infrastructure.Repository;

public class CartRepository(CartDbContext context) : ICartRepository
{
    public async Task<Result<Cart>> GetByCustomerIdAsync(Guid customerId)
    {
        var entity = await context.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (entity == null)
            return Result.Failure<Cart>("Cart not found");

        return Result.Success(MapToDomain(entity));
    }

    public async Task<IReadOnlyList<Cart>> GetPendingCheckoutsStartedBeforeAsync(DateTimeOffset staleBefore)
    {
        var entities = await context.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .Where(c => c.IsCheckoutPending && c.CheckoutPendingSince <= staleBefore)
            .ToListAsync();

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<Result> AddAsync(Cart cart)
    {
        var exists = await context.Carts.AnyAsync(c => c.CustomerId == cart.CustomerId);
        if (exists)
            return Result.Failure("Cart already exists for this customer");

        await context.Carts.AddAsync(MapToEntity(cart));
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(Cart cart)
    {
        var entity = await context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CartId == cart.CartId);

        if (entity == null)
            return Result.Failure("Cart not found");

        entity.IsCheckoutPending = cart.IsCheckoutPending;
        entity.PendingCheckoutId = cart.PendingCheckoutId;
        entity.CheckoutPendingSince = cart.CheckoutPendingSince;

        var existingById = entity.Items.ToDictionary(i => i.CartItemId);
        var domainById = cart.Items.ToDictionary(i => i.CartItemId);

        foreach (var removed in existingById.Values.Where(i => !domainById.ContainsKey(i.CartItemId)))
            entity.Items.Remove(removed);

        foreach (var added in cart.Items.Where(i => !existingById.ContainsKey(i.CartItemId)))
        {
            var itemEntity = MapItemToEntity(added, cart.CartId);
            entity.Items.Add(itemEntity);
        }

        foreach (var domainItem in cart.Items.Where(i => existingById.ContainsKey(i.CartItemId)))
        {
            var existing = existingById[domainItem.CartItemId];
            existing.Quantity = domainItem.Quantity;
            existing.Price = domainItem.Price;
            existing.ProductName = domainItem.ProductName;
        }

        return Result.Success();
    }

    private static CartEntity MapToEntity(Cart cart) => new()
    {
        CartId = cart.CartId,
        CustomerId = cart.CustomerId,
        IsCheckoutPending = cart.IsCheckoutPending,
        PendingCheckoutId = cart.PendingCheckoutId,
        CheckoutPendingSince = cart.CheckoutPendingSince,
        Items = cart.Items.Select(i => MapItemToEntity(i, cart.CartId)).ToList()
    };

    private static Cart MapToDomain(CartEntity entity)
    {
        var cart = Cart.Reconstitute(
            entity.CartId,
            entity.CustomerId,
            entity.IsCheckoutPending,
            entity.PendingCheckoutId,
            entity.CheckoutPendingSince);

        var items = entity.Items.Select(i =>
            CartItem.Reconstitute(i.CartItemId, i.ProductId, i.ProductName, i.Price, i.Quantity));

        cart.LoadItems(items);
        return cart;
    }

    private static CartItemEntity MapItemToEntity(CartItem item, Guid cartId) => new()
    {
        CartItemId = item.CartItemId,
        CartId = cartId,
        ProductId = item.ProductId,
        ProductName = item.ProductName,
        Price = item.Price,
        Quantity = item.Quantity
    };
}
