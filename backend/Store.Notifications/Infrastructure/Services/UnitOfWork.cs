using Store.Notifications.Application.Interfaces;
using Store.Notifications.Infrastructure.DbContexts;

namespace Store.Notifications.Infrastructure.Services;

public class UnitOfWork(NotificationsDbContext dbContext) : INotificationsUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
