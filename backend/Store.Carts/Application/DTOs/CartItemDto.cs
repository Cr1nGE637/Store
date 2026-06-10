namespace Store.Carts.Application.DTOs;

public record CartItemDto(
    Guid CartItemId,
    Guid ProductId,
    string ProductName,
    CartItemImageDto? MainImage,
    decimal Price,
    int Quantity);
