using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Configuration;

public class CustomersConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
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