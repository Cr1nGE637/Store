using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Domain.Entities;
using Store.Infrastructure.DbContexts;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Repositories;

public class CustomersRepository
{
    private readonly StoreDbContext _dbContext;
    private readonly IMapper _mapper;
    public CustomersRepository(StoreDbContext dbContext,  IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        var customerEntity = _mapper.Map<CustomerEntity>(customer);
        await _dbContext.Customers.AddAsync(customerEntity);
    }

    public async Task<Customer?> GetCustomerByIdAsync(Guid id)
    {
        var customer = await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return customer != null ? _mapper.Map<Customer>(customer) : null;
    }
    
    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        var customers = await _dbContext.Customers.AsNoTracking().ToListAsync();
        return _mapper.Map<List<Customer>>(customers);
    }

    public async Task<Result<Customer>> UpdateCustomerAsync(Customer customer)
    {
        var existingCustomer = await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customer.Id);
        if (existingCustomer == null)
        {
            return Result.Failure<Customer>($"Customer with id {customer.Id} not found");
        }

        if (existingCustomer.Email != customer.Email)
        {
            var existingEmail = await _dbContext.Customers.AsNoTracking().AnyAsync(c => c.Email == customer.Email);
            if (existingEmail)
            {
                return Result.Failure<Customer>($"Email {customer.Email} already exists");
            }
        }
        existingCustomer.Email = customer.Email;
        _dbContext.Customers.Update(existingCustomer);
        await _dbContext.SaveChangesAsync();
        return Result.Success<Customer>(_mapper.Map<Customer>(customer));
    }

    public async Task<Result<Customer>> DeleteCustomerAsync(Guid id)
    {
        var customer = await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null)
        {
            return Result.Failure<Customer>($"Customer with id {id} not found");
        }
        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync();
        return Result.Success<Customer>(_mapper.Map<Customer>(customer));
    }
}