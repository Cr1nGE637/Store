using CSharpFunctionalExtensions;
using Store.Domain.Aggregates;
using Store.Domain.Entities;

namespace Store.Domain.Interfaces;

public interface ICustomersRepository
{
    Task<Result<Customer>> AddAsync(Customer customer);
    Task<Result<Customer>> GetByIdAsync(Guid id);
    Task<Result<Customer>> GetByEmailAsync(string email);
    Task<Result<List<Customer>>> GetAllAsync();
    Task<Result<Customer>> UpdateAsync(Customer customer);
    Task<Result<Customer>> DeleteAsync(Guid id);
}