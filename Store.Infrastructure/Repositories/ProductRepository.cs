using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Domain.Entities;
using Store.Domain.Interfaces;
using Store.Infrastructure.DbContexts;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly StoreDbContext _storeDbContext;
    private readonly IMapper _mapper;
    public ProductRepository(StoreDbContext storeDbContext,  IMapper mapper)
    {
        _storeDbContext = storeDbContext;
        _mapper = mapper;
    }
    public async Task<Result<List<Product>>> GetAllAsync()
    {
        var products = await _storeDbContext.Products.AsNoTracking().ToListAsync();
        return Result.Success(_mapper.Map<List<Product>>(products));
    }
    public async Task<Product?> GetByNameAsync(string name)
    {
        var product = await _storeDbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Name == name);
        return product != null ? _mapper.Map<Product>(product) : null;
    }
    public async Task<Result<Product>> GetByIdAsync(Guid id)
    {
        var product = await _storeDbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return Result.Failure<Product>("Product not found");
        }
        
        return Result.Success(_mapper.Map<Product>(product));
    }
    public async Task AddAsync(Product product)
    {
        var productEntity = _mapper.Map<ProductEntity>(product);
        await _storeDbContext.Products.AddAsync(productEntity);
        await _storeDbContext.SaveChangesAsync();
    }
    public async Task<Result> Delete(Guid id)
    {
        var product = await _storeDbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return Result.Failure("Product not found");
        }
        _storeDbContext.Products.Remove(product);
        await _storeDbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<Product>> UpdateProduct(Product product)
    {
        var existingProduct = await _storeDbContext.Products.FirstOrDefaultAsync(p => p.Id == product.Id);

        if (existingProduct == null)
        {
            return Result.Failure<Product>("Product not found");
        }
        
        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        
        _mapper.Map(product, existingProduct);
        await _storeDbContext.SaveChangesAsync();
        
        return Result.Success(_mapper.Map<Product>(existingProduct));
    }
}