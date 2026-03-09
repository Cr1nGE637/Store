using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Infrastructure.Entity;

namespace Users.Infrastructure.Configuration;

public class UsersConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        
        builder.Property(c => c.Email)
            .IsRequired();
        
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}