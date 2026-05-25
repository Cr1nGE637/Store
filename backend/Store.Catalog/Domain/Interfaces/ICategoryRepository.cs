using CSharpFunctionalExtensions;
using Store.Catalog.Domain.Entities;

namespace Store.Catalog.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Result<List<Category>>> GetAllAsync();
    Task<Result<List<Category>>> GetPageAsync(int skip, int take);
    Task<Result<Category>> GetByIdAsync(Guid id);
    Task<Result<Category>> GetByNameAsync(string name);
    Task AddAsync(Category category);
    Task<Result> UpdateAsync(Category category);
    Task<Result> DeleteAsync(Guid id);
}
