namespace Store.Catalog.Application.Interfaces;

public interface ICatalogUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
