namespace Store.Ordering.Application.DTOs;

public record OrderedProductDto(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity);

public record GetOrderDto(
    Guid OrderId,
    Guid CustomerId,
    string RecipientName,
    string Phone,
    string PaymentMethod,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    DateTime? PaidAt,
    decimal? PaidAmount,
    string? PaymentTransactionId,
    DateTime? CancelledAt,
    IReadOnlyList<OrderedProductDto> Products);
