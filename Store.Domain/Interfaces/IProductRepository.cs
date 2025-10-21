using CSharpFunctionalExtensions;
using Store.Domain.Entities;

namespace Store.Domain.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByName(string name);
    Task<Product?> GetProductById(Guid id);
    Task AddProduct(Product product);
    Task<Result> DeleteProduct(Guid id);
    Task<Result<Product>> UpdateProduct(Product product);
}