using CSharpFunctionalExtensions;
using Identity.Domain.Entities;

namespace Identity.Domain.Interfaces;

public interface IUsersRepository
{
    Task<Result> AddAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
    Task<Result<User>> GetByEmailAsync(string email);

}