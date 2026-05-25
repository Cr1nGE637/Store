namespace Store.Identity.Application.Interfaces;

public interface IIdentityUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
