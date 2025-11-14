using CSharpFunctionalExtensions;
using Store.Application.DTOs;
using Store.Domain.Entities;

namespace Store.Application.Interfaces;

public interface IProductService
{
    Task<Result<List<Product>>> GetAllProductsAsync();
    Task<Result<Product>> GetProductById(Guid productId);
    Task<Result> AddProduct(CreateProductDto createProductDto);
    Task<Result> DeleteProduct(Guid productId);
    Task<Result> UpdateProduct(GetProductDto productDto);
}