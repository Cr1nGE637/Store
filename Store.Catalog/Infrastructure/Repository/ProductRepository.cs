using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;
using Store.Catalog.Infrastructure.DbContexts;
using Store.Catalog.Infrastructure.Entity;

namespace Store.Catalog.Infrastructure.Repository;

public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _dbContext;

    public ProductRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<Product>>> GetAllAsync()
    {
        var entities = await _dbContext.Products.AsNoTracking().ToListAsync();
        return Result.Success(entities.Select(ToDomain).ToList());
    }

    public async Task<Result<Product>> GetByNameAsync(string name)
    {
        var entity = await _dbContext.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductName == name);
        if (entity == null)
            return Result.Failure<Product>("Product not found");

        return Result.Success(ToDomain(entity));
    }

    public async Task<Result<Product>> GetByIdAsync(Guid id)
    {
        var entity = await _dbContext.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == id);
        if (entity == null)
            return Result.Failure<Product>("Product not found");

        return Result.Success(ToDomain(entity));
    }

    public async Task AddAsync(Product product)
    {
        await _dbContext.Products.AddAsync(ToEntity(product));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var entity = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == id);
        if (entity == null)
            return Result.Failure("Product not found");

        _dbContext.Products.Remove(entity);
        return Result.Success();
    }

    public async Task<Result<Product>> UpdateAsync(Product product)
    {
        var entity = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
        if (entity == null)
            return Result.Failure<Product>("Product not found");

        entity.ProductName = product.ProductName;
        entity.ProductDescription = product.ProductDescription;
        entity.ProductPrice = product.ProductPrice;
        entity.CategoryId = product.CategoryId;

        return Result.Success(ToDomain(entity));
    }

    public async Task<bool> HasProductsByCategoryAsync(Guid categoryId)
    {
        return await _dbContext.Products.AnyAsync(p => p.CategoryId == categoryId);
    }

    public async Task<Result<List<Product>>> GetByCategoryIdAsync(Guid categoryId)
    {
        var entities = await _dbContext.Products.AsNoTracking()
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
        return Result.Success(entities.Select(ToDomain).ToList());
    }

    private static Product ToDomain(ProductEntity e) =>
        Product.Reconstitute(e.ProductId, e.ProductName, e.ProductDescription, e.ProductPrice, e.CategoryId);

    private static ProductEntity ToEntity(Product p) => new()
    {
        ProductId = p.ProductId,
        ProductName = p.ProductName,
        ProductDescription = p.ProductDescription,
        ProductPrice = p.ProductPrice,
        CategoryId = p.CategoryId
    };
}
