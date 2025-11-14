using CSharpFunctionalExtensions;
using Store.Application.DTOs;
using Store.Domain.Entities;

namespace Store.Application.Interfaces;

public interface ICustomerService
{
    Task<Result<Customer>> GetCustomerByIdAsync(Guid customerId);
    Task<Result<List<Customer>>> GetAllCustomersAsync();
    Task<Result<Customer>> CreateCustomerAsync(Guid customerId, CreateCustomerDto customerDto);
    Task<Result<Customer>> UpdateCustomerAsync(UpdateCustomerDto updateCustomerDto);
}