using CSharpFunctionalExtensions;
using Store.Application.DTOs;
using Store.Domain.Entities;

namespace Store.Application.Services;

public interface IProductService
{
    Task<Result<List<Product>>> GetAllProductsAsync();
    Task<Result<Product>> GetProductById(Guid productId);
    Task<Result> AddProduct(CreateProductRequest createProductRequest);
    Task<Result> DeleteProduct(Guid productId);
    Task<Result> UpdateProduct(UpdateProductRequest productRequest);
}