using CSharpFunctionalExtensions;
using Store.Domain.Aggregates;

namespace Store.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid orderId);
    Task AddAsync(Order order);
    Task<Result<Order>> UpdateAsync(Order order);
    Task<Result> DeleteAsync(Order order);
}