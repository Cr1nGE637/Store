using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Catalog.Infrastructure.Entity;

namespace Store.Catalog.Infrastructure.Configuration;

public class CategoryConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.HasKey(c => c.CategoryId);
        builder.Property(c => c.CategoryId).ValueGeneratedOnAdd().IsRequired();
        builder.Property(c => c.CategoryName).IsRequired().HasMaxLength(100);
    }
}
