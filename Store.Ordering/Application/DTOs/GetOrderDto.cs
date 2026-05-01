namespace Store.Ordering.Application.DTOs;

public record OrderedProductDto(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity);

public record GetOrderDto(
    Guid OrderId,
    Guid CustomerId,
    string Status,
    DateTime CreatedAt,
    DateTime? PaidAt,
    DateTime? CancelledAt,
    IReadOnlyList<OrderedProductDto> Products);
