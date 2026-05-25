namespace Store.Carts.Application.Interfaces;

public interface ICartUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
