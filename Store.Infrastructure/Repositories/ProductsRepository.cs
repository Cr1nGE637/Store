using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Domain.Entities;
using Store.Domain.Interfaces;
using Store.Infrastructure.DbContexts;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Repositories;
public class ProductsRepository : IProductsRepository
{
    private readonly StoreDbContext _storeDbContext;
    private readonly IMapper _mapper;
    public ProductsRepository(StoreDbContext storeDbContext,  IMapper mapper)
    {
        _storeDbContext = storeDbContext;
        _mapper = mapper;
    }
    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = await _storeDbContext.Products.AsNoTracking().ToListAsync();
        return _mapper.Map<List<Product>>(products);
    }
    public async Task<Product?> GetByNameProduct(string name)
    {
        var product = await _storeDbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Name == name);
        return product != null ? _mapper.Map<Product>(product) : null;
    }
    public async Task<Product?> GetByIdProduct(Guid id)
    {
        var product = await _storeDbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return product != null ? _mapper.Map<Product>(product) : null;
    }
    public async Task AddProduct(Product product)
    {
        var productEntity = _mapper.Map<ProductEntity>(product);
        await _storeDbContext.Products.AddAsync(productEntity);
    }
    public async Task<Result> DeleteProduct(Guid id)
    {
        var product = await _storeDbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return Result.Failure("Product not found");
        }
        _storeDbContext.Products.Remove(product);
        return Result.Success(_mapper.Map<Product>(product));
    }

    public async Task<Result<Product>> UpdateProduct(Product product)
    {
        var existingProduct = await _storeDbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);

        if (existingProduct == null)
        {
            return Result.Failure<Product>("Product not found");
        }

        if (existingProduct.Name != product.Name)
        {
            var existingProductName = _storeDbContext.Products.Any(p => p.Name == product.Name);
            if (existingProductName)
            {
                return Result.Failure<Product>("Product name already exists");
            }
        }
        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        existingProduct.Description = product.Description;
        
        return Result.Success(_mapper.Map<Product>(existingProduct));
    }
}