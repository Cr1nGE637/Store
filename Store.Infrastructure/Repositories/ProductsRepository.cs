using Microsoft.EntityFrameworkCore;
using Store.Domain.Entities;
using Store.Domain.Interfaces;
using Store.Infrastructure.DbContexts;

namespace Store.Infrastructure.Repositories;

public class ProductsRepository : IProductsRepository
{
    private readonly StoreDbContext _storeDbContext;
    

    public ProductsRepository(StoreDbContext storeDbContext)
    {
        _storeDbContext = storeDbContext;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = await _storeDbContext.Products.AsNoTracking().ToListAsync();
        return products;
    }

    public async Task<Product?> GetByNameProduct(string name)
    {
        var product = await _storeDbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Name == name);
        return product;
    }
    
}