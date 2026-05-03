using CSharpFunctionalExtensions;
using Store.Identity.Domain.Aggregates;

namespace Store.Identity.Domain.Interfaces;

public interface IUsersRepository
{
    Task<Result> AddAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
    Task<Result<User>> GetByEmailAsync(string email);

}