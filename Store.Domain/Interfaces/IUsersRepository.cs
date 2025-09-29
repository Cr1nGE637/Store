using Store.Domain.Entities;

namespace Store.Domain.Interfaces;

public interface IUsersRepository
{
    Task<User?> GetUserAsync(Guid id);
}