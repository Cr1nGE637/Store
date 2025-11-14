using CSharpFunctionalExtensions;
using Store.Application.DTOs;
using Store.Application.Interfaces;
using Store.Domain.Entities;
using Store.Domain.Interfaces;
using Store.Domain.ValueObjects;

namespace Store.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomersRepository _customersRepository;

    public CustomerService(ICustomersRepository customersRepository)
    {
        _customersRepository =  customersRepository;
    }
    
    public async Task<Result<Customer>> GetCustomerByIdAsync(Guid customerId)
    {
        var customer = await _customersRepository.GetByIdAsync(customerId);
        return customer.IsSuccess ? Result.Success(customer.Value) : Result.Failure<Customer>(customer.Error);
    }

    public async Task<Result<List<Customer>>> GetAllCustomersAsync()
    {
        var customers = await _customersRepository.GetAllAsync();
        return customers.IsSuccess ? Result.Success(customers.Value) : Result.Failure<List<Customer>>(customers.Error);
    }

    public async Task<Result<Customer>> CreateCustomerAsync(Guid customerId, CreateCustomerDto customerDto)
    {
        var existingCustomer = await _customersRepository.GetByIdAsync(customerId);
        if (existingCustomer.IsSuccess)
        {
            return Result.Failure<Customer>("Customer already exists");
        }

        var customerResult = Customer.Create(customerDto.Name, Email.Create(customerDto.Email).Value);
        if (customerResult.IsFailure)
        {
            return Result.Failure<Customer>(customerResult.Error);
        }
        
        var customer = customerResult.Value;
        await _customersRepository.AddAsync(customer);
        return Result.Success(customer);
    }

    public async Task<Result<Customer>> UpdateCustomerAsync(UpdateCustomerDto updateCustomerDto)
    {
        var customer = await _customersRepository.GetByIdAsync(updateCustomerDto.Id);
        if (customer.IsFailure)
        {
            return Result.Failure<Customer>("Customer not found");
        }
        
        var customerResult = await _customersRepository.UpdateAsync(customer.Value);
        if (customerResult.IsFailure)
        {
            return Result.Failure<Customer>(customerResult.Error);
        }
        var updatedCustomer = customerResult.Value;
        return Result.Success(updatedCustomer);
    }
}