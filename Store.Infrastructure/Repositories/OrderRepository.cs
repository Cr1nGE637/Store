using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Domain.Aggregates;
using Store.Domain.Interfaces;
using Store.Domain.ValueObjects;
using Store.Infrastructure.DbContexts;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly StoreDbContext _dbContext;
    private readonly IMapper _mapper;

    public OrderRepository(StoreDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Order> GetByIdAsync(Guid orderId)
    {
        var order = await _dbContext.Orders
            .Include(o => o.OrderedProducts)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        return _mapper.Map<Order>(order);
    }
    
    public async Task AddAsync(Order order)
    {
        var orderEntity = _mapper.Map<OrderEntity>(order);
        _dbContext.Add(orderEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Result<Order>> UpdateAsync(Order order)
    {
        var existingOrder = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == order.OrderId);
        if (existingOrder == null)
        {
            return Result.Failure<Order>("Order not found");
        }
        var orderEntity = _mapper.Map<OrderEntity>(order);
        _dbContext.Orders.Update(orderEntity);
        await _dbContext.SaveChangesAsync();
        return Result.Success(_mapper.Map<Order>(existingOrder));
    }

    public async Task<Result> DeleteAsync(Order order)
    {
        var existingOrder = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == order.OrderId);
        if (existingOrder == null)
        {
            return Result.Failure("Order not found"); 
        }
        _dbContext.Remove(existingOrder);
        await _dbContext.SaveChangesAsync();
        return Result.Success();
    }
}