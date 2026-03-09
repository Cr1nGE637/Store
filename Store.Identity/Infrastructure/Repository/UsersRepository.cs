using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Users.Application.Interfaces;
using Users.Domain.Entities;
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
        var userEntity = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
        return userEntity != null;
    }
    public async Task<Result<User>> GetByEmailAsync(string email)
    {
        var userEntity = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email);
        if (userEntity == null)
        {
            return Result.Failure<User>("Email not found");
        }
        var emailVo = Email.Create(email);
        var user = User.Create(userEntity.Name, emailVo.Value, userEntity.Password);
        return Result.Success(user.Value);
    }


}