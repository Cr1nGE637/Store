namespace Store.Carts.Application.DTOs;

public record GetCartDto(
    Guid CartId,
    Guid CustomerId,
    bool IsCheckoutPending,
    IReadOnlyList<CartItemDto> Items);
