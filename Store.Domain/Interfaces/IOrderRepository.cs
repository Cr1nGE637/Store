using CSharpFunctionalExtensions;
using Store.Domain.Aggregates;

namespace Store.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Result<Order>> GetByIdAsync(Guid orderId);
    Task<Result<Order>> CreateAsync(Order order);
    Task<Result<Order>> UpdateAsync(Order order);
    Task<Result> DeleteAsync(Order order);
}