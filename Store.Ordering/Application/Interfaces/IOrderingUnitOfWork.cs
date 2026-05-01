namespace Store.Ordering.Application.Interfaces;

public interface IOrderingUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
