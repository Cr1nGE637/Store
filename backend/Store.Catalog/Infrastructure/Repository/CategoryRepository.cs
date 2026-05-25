using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;
using Store.Catalog.Infrastructure.DbContexts;
using Store.Catalog.Infrastructure.Entity;

namespace Store.Catalog.Infrastructure.Repository;

public class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _dbContext;

    public CategoryRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<Category>>> GetAllAsync()
    {
        var entities = await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
        return Result.Success(entities.Select(ToDomain).ToList());
    }

    public async Task<Result<List<Category>>> GetPageAsync(int skip, int take)
    {
        var entities = await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return Result.Success(entities.Select(ToDomain).ToList());
    }

    public async Task<Result<Category>> GetByIdAsync(Guid id)
    {
        var entity = await _dbContext.Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == id);
        if (entity == null)
            return Result.Failure<Category>("Category not found");

        return Result.Success(ToDomain(entity));
    }

    public async Task<Result<Category>> GetByNameAsync(string name)
    {
        var normalizedName = name.Trim();
        var entity = await _dbContext.Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => EF.Functions.ILike(c.CategoryName, normalizedName));
        if (entity == null)
            return Result.Failure<Category>("Category not found");

        return Result.Success(ToDomain(entity));
    }

    public async Task AddAsync(Category category)
    {
        await _dbContext.Categories.AddAsync(ToEntity(category));
    }

    public async Task<Result> UpdateAsync(Category category)
    {
        var entity = await _dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);
        if (entity == null)
            return Result.Failure("Category not found");

        entity.CategoryName = category.CategoryName;
        entity.CategoryCode = category.CategoryCode.ToString();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var entity = await _dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
        if (entity == null)
            return Result.Failure("Category not found");

        _dbContext.Categories.Remove(entity);
        return Result.Success();
    }

    private static Category ToDomain(CategoryEntity e) =>
        Category.Reconstitute(e.CategoryId, e.CategoryName, e.CategoryCode);

    private static CategoryEntity ToEntity(Category c) => new()
    {
        CategoryId = c.CategoryId,
        CategoryName = c.CategoryName,
        CategoryCode = c.CategoryCode.ToString()
    };
}
