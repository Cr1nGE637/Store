using CSharpFunctionalExtensions;
using Users.Domain.Entities;

namespace Users.Application.Interfaces;

public interface IUsersRepository
{
    Task<Result> AddAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
    Task<Result<User>> GetByEmailAsync(string email);

}