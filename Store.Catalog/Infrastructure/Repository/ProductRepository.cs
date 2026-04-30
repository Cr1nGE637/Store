using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;
using Store.Catalog.Infrastructure.DbContext;
using Store.Catalog.Infrastructure.Entity;

namespace Store.Catalog.Infrastructure.Repository;

public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _dbContext;
    private readonly IMapper _mapper;

    public ProductRepository(CatalogDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Result<List<Product>>> GetAllAsync()
    {
        var entities = await _dbContext.Products.AsNoTracking().ToListAsync();
        return Result.Success(_mapper.Map<List<Product>>(entities));
    }

    public async Task<Product?> GetByNameAsync(string name)
    {
        var entity = await _dbContext.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductName == name);
        return entity != null ? _mapper.Map<Product>(entity) : null;
    }

    public async Task<Result<Product>> GetByIdAsync(Guid id)
    {
        var entity = await _dbContext.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == id);
        if (entity == null)
            return Result.Failure<Product>("Product not found");

        return Result.Success(_mapper.Map<Product>(entity));
    }

    public async Task AddAsync(Product product)
    {
        var entity = _mapper.Map<ProductEntity>(product);
        await _dbContext.Products.AddAsync(entity);
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

        return Result.Success(_mapper.Map<Product>(entity));
    }
}