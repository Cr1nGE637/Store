namespace Store.Catalog.Application.DTOs;

public record CreateProductDto(string ProductName, string ProductDescription, decimal ProductPrice);