namespace Store.Carts.Application.DTOs;

public record CartItemDto(
    Guid CartItemId,
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity);
