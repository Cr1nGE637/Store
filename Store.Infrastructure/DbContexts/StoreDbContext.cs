using Microsoft.EntityFrameworkCore;
using Store.Domain.Entities;
namespace Store.Infrastructure.DbContexts;

public class StoreDbContext(DbContextOptions<StoreDbContext> options) :  DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }

    public void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoreDbContext).Assembly);
    }
}