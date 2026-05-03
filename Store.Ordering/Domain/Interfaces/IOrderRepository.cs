using CSharpFunctionalExtensions;
using Store.Ordering.Domain.Aggregates;

namespace Store.Ordering.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Result<Order>> GetByIdAsync(Guid orderId);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId);
    Task<Result> AddAsync(Order order);
    Task<Result> UpdateAsync(Order order);
}
