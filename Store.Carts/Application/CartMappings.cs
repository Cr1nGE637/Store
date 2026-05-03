using Store.Carts.Application.DTOs;
using Store.Carts.Domain.Aggregates;
using Store.Carts.Domain.Entities;

namespace Store.Carts.Application;

internal static class CartMappings
{
    internal static GetCartDto ToGetCartDto(Cart cart) => new(
        cart.CartId,
        cart.CustomerId,
        cart.Items.Select(ToCartItemDto).ToList());

    internal static CheckoutResultDto ToCheckoutResultDto(Cart cart, IReadOnlyList<CartItem> items) => new(
        cart.CartId,
        cart.CustomerId,
        items.Select(ToCartItemDto).ToList());

    private static CartItemDto ToCartItemDto(CartItem i) =>
        new(i.CartItemId, i.ProductId, i.ProductName, i.Price, i.Quantity);
}
