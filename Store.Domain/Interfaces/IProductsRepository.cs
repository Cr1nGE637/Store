using CSharpFunctionalExtensions;
using Store.Domain.Entities;

namespace Store.Domain.Interfaces;

public interface IProductsRepository
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetByNameProduct(string name);
    Task<Product?> GetByIdProduct(Guid id);
    Task AddProduct(Product product);
    Task<Result> DeleteProduct(Guid id);
    Task<Result<Product>> UpdateProduct(Product product);
}