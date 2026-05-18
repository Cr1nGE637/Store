using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Catalog.Infrastructure.Entity;

namespace Store.Catalog.Infrastructure.Configuration;

public class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecificationEntity>
{
    public void Configure(EntityTypeBuilder<ProductSpecificationEntity> builder)
    {
        builder.HasKey(s => new { s.ProductId, s.Name });

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasIndex(s => new { s.Name, s.Value });

        builder.ToTable("ProductSpecifications");
    }
}
