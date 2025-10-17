using CSharpFunctionalExtensions;
using Store.Domain.Aggregates;
using Store.Domain.Interfaces;

namespace Store.Application.Services;

public class OrderService : IOrderRepository
{
    public Task<Order> GetByIdAsync(Guid orderId)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Order order)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Order>> UpdateAsync(Order order)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteAsync(Order order)
    {
        throw new NotImplementedException();
    }
}