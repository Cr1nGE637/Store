using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Entities;

namespace Store.Catalog.Application;

internal static class CatalogMappings
{
    internal static GetProductDto ToGetProductDto(Product p, int availableQuantity = 0) =>
        new(
            p.ProductId,
            p.Sku.Value,
            p.ProductName,
            p.ProductDescription,
            p.ProductPrice,
            p.Brand,
            p.Model,
            p.WarrantyMonths,
            p.CategoryId,
            p.Specifications.ToDictionary(s => s.Name, s => s.Value),
            availableQuantity,
            availableQuantity > 0);

    internal static CreateProductDto ToCreateProductDto(Product p) =>
        new(
            p.ProductId,
            p.Sku.Value,
            p.ProductName,
            p.ProductDescription,
            p.ProductPrice,
            p.Brand,
            p.Model,
            p.WarrantyMonths,
            p.CategoryId,
            p.Specifications.ToDictionary(s => s.Name, s => s.Value));

    internal static GetCategoryDto ToGetCategoryDto(Category c) => new(c.CategoryId, c.CategoryName);

    internal static CreateCategoryDto ToCreateCategoryDto(Category c) => new(c.CategoryId, c.CategoryName);
}
