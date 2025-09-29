using Store.Domain.Entities;

namespace Store.Domain.Interfaces;

public interface IProductsRepository
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetByNameProduct(string name);
}