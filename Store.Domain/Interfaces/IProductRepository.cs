using CSharpFunctionalExtensions;
using Store.Domain.Entities;

namespace Store.Domain.Interfaces;

public interface IProductRepository
{
    Task<Result<List<Product>>> GetAllAsync();
    Task<Product?> GetByNameAsync(string name);
    Task<Result<Product>> GetByIdAsync(Guid id);
    Task AddAsync(Product product);
    Task<Result> Delete(Guid id);
    Task<Result<Product>> UpdateProduct(Product product);
}