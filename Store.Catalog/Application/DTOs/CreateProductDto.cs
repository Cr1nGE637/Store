namespace Store.Catalog.Application.DTOs;

public record CreateProductDto(Guid ProductId, string ProductName, string ProductDescription, decimal ProductPrice, Guid CategoryId);