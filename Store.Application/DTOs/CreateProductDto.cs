namespace Store.Application.DTOs;

public record CreateProductDto(string ProductName, string ProductDescription, decimal ProductPrice);