using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Entities;

namespace Store.Catalog.Application;

internal static class CatalogMappings
{
    internal static GetProductDto ToGetProductDto(Product p) =>
        new(p.ProductId, p.ProductName, p.ProductDescription, p.ProductPrice, p.CategoryId);

    internal static CreateProductDto ToCreateProductDto(Product p) =>
        new(p.ProductId, p.ProductName, p.ProductDescription, p.ProductPrice, p.CategoryId);

    internal static GetCategoryDto ToGetCategoryDto(Category c) => new(c.CategoryId, c.CategoryName);

    internal static CreateCategoryDto ToCreateCategoryDto(Category c) => new(c.CategoryId, c.CategoryName);
}
