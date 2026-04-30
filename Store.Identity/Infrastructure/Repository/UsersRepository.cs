using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Domain.Interfaces;
using Users.Domain.ValueObjects;
using Users.Infrastructure.DbContext;
using Users.Infrastructure.Entity;

namespace Users.Infrastructure.Repository;

public class UsersRepository : IUsersRepository
{
    private readonly IdentityDbContext _dbContext;
    private readonly IMapper _mapper;
    
    public UsersRepository(IdentityDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Result> AddAsync(User user) 
    {
        var userEntity = _mapper.Map<UserEntity>(user);
        await _dbContext.Users.AddAsync(userEntity);
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
        var userEntity = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == normalized);
        if (userEntity == null)
            return Result.Failure<User>("Email not found");

        var emailVo = Email.Create(userEntity.Email);
        var role = Enum.Parse<UserRole>(userEntity.Role);
        var user = User.Create(userEntity.Name, emailVo.Value, userEntity.Password, role, userEntity.Id);
        return Result.Success(user.Value);
    }


}