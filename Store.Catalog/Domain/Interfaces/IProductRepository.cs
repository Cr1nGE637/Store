using CSharpFunctionalExtensions;
using Store.Catalog.Domain.Entities;

namespace Store.Catalog.Domain.Interfaces;

public interface IProductRepository
{
    Task<Result<List<Product>>> GetAllAsync();
    Task<Result<List<Product>>> GetPageAsync(int skip, int take);
    Task<Result<List<Product>>> SearchAsync(ProductSearchCriteria criteria);
    Task<Result<Product>> GetByNameAsync(string name);
    Task<Result<Product>> GetBySkuAsync(string sku);
    Task<Result<Product>> GetByIdAsync(Guid id);
    Task AddAsync(Product product);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> UpdateAsync(Product product);
    Task<bool> HasProductsByCategoryAsync(Guid categoryId);
    Task<Result<List<Product>>> GetByCategoryIdAsync(Guid categoryId);
    Task<Result<List<Product>>> GetByCategoryIdAsync(Guid categoryId, int skip, int take);
}
