using Microsoft.EntityFrameworkCore;
using Store.Domain.Entities;
using Store.Domain.Interfaces;
using Store.Infrastructure.DbContexts;

namespace Store.Infrastructure.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly StoreDbContext _storeDbContext;

    public UsersRepository(StoreDbContext storeDbContext)
    {
        _storeDbContext = storeDbContext;
    }

    public async Task<User?> GetUserAsync(Guid id)
    {
        var user = await _storeDbContext.Users.FirstOrDefaultAsync(p => p.Id == id);
        return user;
    }
}