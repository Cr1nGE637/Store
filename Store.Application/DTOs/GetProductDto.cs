namespace Store.Application.DTOs;

public record GetProductDto(Guid ProductId, string? ProductName, string? ProductDescription, decimal ProductPrice);