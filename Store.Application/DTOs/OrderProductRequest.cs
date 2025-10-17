namespace Store.Application.DTOs;

public record OrderProductRequest(Guid ProductId, string? ProductName, decimal ProductPrice, int ProductQuantity);