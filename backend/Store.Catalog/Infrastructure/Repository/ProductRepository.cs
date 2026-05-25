using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;
using Store.Catalog.Domain.ValueObjects;
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
        var entities = await _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Specifications)
            .OrderBy(p => p.ProductName)
            .ToListAsync();
        return Result.Success(entities.Select(ToDomain).ToList());
    }

    public async Task<Result<List<Product>>> GetPageAsync(int skip, int take)
    {
        var entities = await _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Specifications)
            .OrderBy(p => p.ProductName)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return Result.Success(entities.Select(ToDomain).ToList());
    }

    public async Task<Result<List<Product>>> SearchAsync(ProductSearchCriteria criteria)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Specifications)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = $"%{criteria.Search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.ProductName, search)
                || EF.Functions.ILike(p.ProductDescription, search)
                || EF.Functions.ILike(p.Sku, search)
                || EF.Functions.ILike(p.Brand, search)
                || EF.Functions.ILike(p.Model, search));
        }

        if (criteria.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == criteria.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(criteria.Brand))
        {
            var brand = criteria.Brand.Trim();
            query = query.Where(p => EF.Functions.ILike(p.Brand, brand));
        }

        if (criteria.MinPrice.HasValue)
            query = query.Where(p => p.ProductPrice >= criteria.MinPrice.Value);

        if (criteria.MaxPrice.HasValue)
            query = query.Where(p => p.ProductPrice <= criteria.MaxPrice.Value);

        if (criteria.InStockOnly)
            query = query.Where(p => _dbContext.ProductAvailabilities.Any(a =>
                a.ProductId == p.ProductId
                && a.AvailableQuantity > 0));

        foreach (var filter in criteria.SpecificationFilters)
        {
            if (string.IsNullOrWhiteSpace(filter.Key) || string.IsNullOrWhiteSpace(filter.Value))
                continue;

            var name = filter.Key.Trim();
            var value = filter.Value.Trim();
            query = query.Where(p => p.Specifications.Any(s =>
                EF.Functions.ILike(s.Name, name)
                && EF.Functions.ILike(s.Value, value)));
        }

        var entities = await query
            .OrderBy(p => p.ProductName)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync();

        return Result.Success(entities.Select(ToDomain).ToList());
    }

    public async Task<Result<Product>> GetByNameAsync(string name)
    {
        var normalizedName = name.Trim();
        var entity = await _dbContext.Products.AsNoTracking()
            .Include(p => p.Specifications)
            .FirstOrDefaultAsync(p => EF.Functions.ILike(p.ProductName, normalizedName));
        if (entity == null)
            return Result.Failure<Product>("Product not found");

        return Result.Success(ToDomain(entity));
    }

    public async Task<Result<Product>> GetBySkuAsync(string sku)
    {
        var normalizedSku = sku.Trim().ToUpperInvariant();
        var entity = await _dbContext.Products.AsNoTracking()
            .Include(p => p.Specifications)
            .FirstOrDefaultAsync(p => p.Sku == normalizedSku);
        if (entity == null)
            return Result.Failure<Product>("Product not found");

        return Result.Success(ToDomain(entity));
    }

    public async Task<Result<Product>> GetByIdAsync(Guid id)
    {
        var entity = await _dbContext.Products.AsNoTracking()
            .Include(p => p.Specifications)
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

    public async Task<Result> UpdateAsync(Product product)
    {
        var entity = await _dbContext.Products
            .Include(p => p.Specifications)
            .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
        if (entity == null)
            return Result.Failure("Product not found");

        entity.ProductName = product.ProductName;
        entity.ProductDescription = product.ProductDescription;
        entity.ProductPrice = product.ProductPrice;
        entity.Sku = product.Sku.Value;
        entity.Brand = product.Brand;
        entity.Model = product.Model;
        entity.WarrantyMonths = product.WarrantyMonths;
        entity.CategoryId = product.CategoryId;
        ApplySpecifications(entity, product.Specifications);
        return Result.Success();
    }

    public async Task<bool> HasProductsByCategoryAsync(Guid categoryId)
    {
        return await _dbContext.Products.AnyAsync(p => p.CategoryId == categoryId);
    }

    public async Task<Result<List<Product>>> GetByCategoryIdAsync(Guid categoryId)
    {
        var entities = await _dbContext.Products.AsNoTracking()
            .Include(p => p.Specifications)
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.ProductName)
            .ToListAsync();
        return Result.Success(entities.Select(ToDomain).ToList());
    }

    public async Task<Result<List<Product>>> GetByCategoryIdAsync(Guid categoryId, int skip, int take)
    {
        var entities = await _dbContext.Products.AsNoTracking()
            .Include(p => p.Specifications)
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.ProductName)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return Result.Success(entities.Select(ToDomain).ToList());
    }

    private static Product ToDomain(ProductEntity e) =>
        Product.Reconstitute(
            e.ProductId,
            e.Sku,
            e.ProductName,
            e.ProductDescription,
            e.ProductPrice,
            e.Brand,
            e.Model,
            e.WarrantyMonths,
            e.CategoryId,
            e.Specifications.Select(s => ProductSpecification.Reconstitute(s.Name, s.Value)));

    private static ProductEntity ToEntity(Product p) => new()
    {
        ProductId = p.ProductId,
        Sku = p.Sku.Value,
        ProductName = p.ProductName,
        ProductDescription = p.ProductDescription,
        ProductPrice = p.ProductPrice,
        Brand = p.Brand,
        Model = p.Model,
        WarrantyMonths = p.WarrantyMonths,
        CategoryId = p.CategoryId,
        Specifications = p.Specifications.Select(ToSpecificationEntity).ToList()
    };

    private static ProductSpecificationEntity ToSpecificationEntity(ProductSpecification specification) => new()
    {
        Name = specification.Name,
        Value = specification.Value
    };

    private static void ApplySpecifications(ProductEntity entity, IReadOnlyCollection<ProductSpecification> specifications)
    {
        var desired = specifications.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
        var existing = entity.Specifications.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var current in entity.Specifications.ToList())
        {
            if (!desired.ContainsKey(current.Name))
                entity.Specifications.Remove(current);
        }

        foreach (var specification in desired.Values)
        {
            if (existing.TryGetValue(specification.Name, out var current))
            {
                current.Value = specification.Value;
                continue;
            }

            entity.Specifications.Add(ToSpecificationEntity(specification));
        }
    }
}
