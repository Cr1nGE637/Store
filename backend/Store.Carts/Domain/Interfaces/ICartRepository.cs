using CSharpFunctionalExtensions;
using Store.Carts.Domain.Aggregates;

namespace Store.Carts.Domain.Interfaces;

public interface ICartRepository
{
    Task<Result<Cart>> GetByCustomerIdAsync(Guid customerId);
    Task<IReadOnlyList<Cart>> GetPendingCheckoutsStartedBeforeAsync(DateTimeOffset staleBefore);
    Task<Result> AddAsync(Cart cart);
    Task<Result> UpdateAsync(Cart cart);
}
