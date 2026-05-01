using Identity.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.DbContexts;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) :  DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}