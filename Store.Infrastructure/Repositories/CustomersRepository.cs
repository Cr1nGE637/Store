using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Domain.Aggregates;
using Store.Domain.Entities;
using Store.Domain.Interfaces;
using Store.Infrastructure.DbContexts;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Repositories;

public class CustomersRepository : ICustomersRepository {
    private readonly StoreDbContext _dbContext;
    private readonly IMapper _mapper;

    public CustomersRepository(StoreDbContext dbContext, IMapper mapper) {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Result<Customer>> AddAsync(Customer customer) {
        var customerEntity = _mapper.Map<CustomerEntity>(customer);
        await _dbContext.Customers.AddAsync(customerEntity);
        await _dbContext.SaveChangesAsync();
        return Result.Success<Customer>(_mapper.Map<Customer>(customerEntity));
    }

    public async Task<Result<Customer>> GetByIdAsync(Guid id) {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        return Result.Success<Customer>(_mapper.Map<Customer>(customer));
    }

    public async Task<Result<Customer>> GetByEmailAsync(string email)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email);
        if (customer == null)
        {
            return Result.Failure<Customer>($"Customer with email {email} not found");
        }
        return Result.Success(_mapper.Map<Customer>(customer));
    }
    public async Task<Result<List<Customer>>> GetAllAsync() {
        var customers = await _dbContext.Customers.AsNoTracking().ToListAsync();
        return Result.Success(_mapper.Map<List<Customer>>(customers));
    }

    public async Task<Result<Customer>> UpdateAsync(Customer customer)
    {
        var existingCustomer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        if (existingCustomer == null) {
            return Result.Failure<Customer>($"Customer with id {customer.Id} not found");
        }

        if (existingCustomer.Email != customer.Email) {
            var existingEmail = await _dbContext.Customers.AnyAsync(c => c.Email == customer.Email);
            if (existingEmail) {
                return Result.Failure<Customer>($"Email {customer.Email} already exists");
            }
        }

        existingCustomer.Email = customer.Email;
        _dbContext.Customers.Update(existingCustomer);
        await _dbContext.SaveChangesAsync();
        return Result.Success<Customer>(_mapper.Map<Customer>(customer));
    }

    public async Task<Result<Customer>> DeleteAsync(Guid id)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null)
        {
            return Result.Failure<Customer>($"Customer with id {id} not found");
        }

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync();
        return Result.Success<Customer>(_mapper.Map<Customer>(customer));
    }


}