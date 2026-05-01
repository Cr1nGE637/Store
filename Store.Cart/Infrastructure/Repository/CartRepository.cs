using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Carts.Domain.Aggregates;
using Store.Carts.Domain.Entities;
using Store.Carts.Domain.Interfaces;
using Store.Carts.Infrastructure.DbContexts;
using Store.Carts.Infrastructure.Entities;

namespace Store.Carts.Infrastructure.Repository;

public class CartRepository(CartDbContext context) : ICartRepository
{
    public async Task<Result<Cart>> GetByCustomerIdAsync(Guid customerId)
    {
        var entity = await context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (entity == null)
            return Result.Failure<Cart>("Cart not found");

        var cart = Cart.Reconstitute(entity.CartId, entity.CustomerId);

        var items = entity.Items.Select(i =>
            CartItem.Reconstitute(i.CartItemId, i.ProductId, i.ProductName, i.Price, i.Quantity));

        cart.LoadItems(items);
        return Result.Success(cart);
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

        entity.Items.Clear();
        entity.Items.AddRange(cart.Items.Select(i => MapItemToEntity(i, cart.CartId)));
        return Result.Success();
    }

    private static CartEntity MapToEntity(Cart cart) => new()
    {
        CartId = cart.CartId,
        CustomerId = cart.CustomerId,
        Items = cart.Items.Select(i => MapItemToEntity(i, cart.CartId)).ToList()
    };

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
