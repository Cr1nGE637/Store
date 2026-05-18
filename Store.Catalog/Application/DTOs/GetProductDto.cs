namespace Store.Catalog.Application.DTOs;

public record GetProductDto(
    Guid ProductId,
    string Sku,
    string ProductName,
    string ProductDescription,
    decimal ProductPrice,
    string Brand,
    string Model,
    int WarrantyMonths,
    Guid CategoryId,
    IReadOnlyDictionary<string, string> Specifications);
