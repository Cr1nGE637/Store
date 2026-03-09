using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Entity;

namespace Users.Infrastructure.DbContext;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) :  Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
    }
}