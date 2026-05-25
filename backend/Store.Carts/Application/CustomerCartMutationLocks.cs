using System.Collections.Concurrent;

namespace Store.Carts.Application;

internal static class CustomerCartMutationLocks
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> Locks = new();

    public static async Task<IDisposable> AcquireAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var semaphore = Locks.GetOrAdd(customerId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        return new Releaser(semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose() => semaphore.Release();
    }
}
