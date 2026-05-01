namespace Store.Carts.Application.DTOs;

public record CheckoutResultDto(
    Guid CartId,
    Guid CustomerId,
    IReadOnlyList<CartItemDto> Items);
