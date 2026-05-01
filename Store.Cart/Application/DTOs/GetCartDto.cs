namespace Store.Carts.Application.DTOs;

public record GetCartDto(
    Guid CartId,
    Guid CustomerId,
    IReadOnlyList<CartItemDto> Items);
