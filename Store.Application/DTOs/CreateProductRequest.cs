namespace Store.Application.DTOs;

public record CreateProductRequest(string ProductName, string ProductDescription, decimal ProductPrice);