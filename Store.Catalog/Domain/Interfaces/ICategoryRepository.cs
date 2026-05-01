using CSharpFunctionalExtensions;
using Store.Catalog.Domain.Entities;

namespace Store.Catalog.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Result<List<Category>>> GetAllAsync();
    Task<Result<Category>> GetByIdAsync(Guid id);
    Task AddAsync(Category category);
    Task<Result<Category>> UpdateAsync(Category category);
    Task<Result> DeleteAsync(Guid id);
}
