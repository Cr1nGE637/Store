namespace Store.Application.DTOs;

public record UpdateProductRequest(Guid ProductId, string? ProductName, decimal ProductPrice);