using CSharpFunctionalExtensions;
using Store.Ordering.Domain.Aggregates;

namespace Store.Ordering.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Result<Order>> GetByIdAsync(Guid orderId);
    Task<Result<Order>> GetBySourceCheckoutIdAsync(Guid sourceCheckoutId);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, int skip, int take);
    Task<Result> AddAsync(Order order);
    Task<Result> UpdateAsync(Order order);
}
