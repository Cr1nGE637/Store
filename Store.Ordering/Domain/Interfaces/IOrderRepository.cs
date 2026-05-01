using CSharpFunctionalExtensions;
using Store.Ordering.Domain.Aggregates;

namespace Store.Ordering.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Result<Order>> GetByIdAsync(Guid orderId);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
}
