using CSharpFunctionalExtensions;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.DbContexts;
using Identity.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repository;

public class UsersRepository : IUsersRepository
{
    private readonly IdentityDbContext _dbContext;

    public UsersRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> AddAsync(User user)
    {
        await _dbContext.Users.AddAsync(ToEntity(user));
        return Result.Success();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalized);
    }

    public async Task<Result<User>> GetByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var entity = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalized);

        if (entity == null)
            return Result.Failure<User>("Email not found");

        var emailVo = Email.Create(entity.Email);
        var role = Enum.Parse<UserRole>(entity.Role);
        return Result.Success(User.Reconstitute(entity.Id, entity.Name, emailVo.Value, entity.Password, role));
    }

    private static UserEntity ToEntity(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Password = user.Password,
        Role = user.Role.ToString()
    };
}
