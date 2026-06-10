using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Catalog.Infrastructure.Entity;

namespace Store.Catalog.Infrastructure.Configuration;

public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImageEntity>
{
    public void Configure(EntityTypeBuilder<ProductImageEntity> builder)
    {
        builder.ToTable("ProductImages");
        builder.HasKey(image => image.ProductImageId);
        builder.Property(image => image.ProductImageId).ValueGeneratedNever().IsRequired();
        builder.Property(image => image.ProductId).IsRequired();
        builder.Property(image => image.Url).IsRequired().HasMaxLength(500);
        builder.Property(image => image.StoragePath).IsRequired().HasMaxLength(500);
        builder.Property(image => image.OriginalFileName).IsRequired().HasMaxLength(255);
        builder.Property(image => image.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(image => image.SizeBytes).IsRequired();
        builder.Property(image => image.AltText).IsRequired().HasMaxLength(200);
        builder.Property(image => image.IsMain).IsRequired();
        builder.Property(image => image.DisplayOrder).IsRequired();
        builder.Property(image => image.CreatedAtUtc).IsRequired();

        builder.HasIndex(image => image.ProductId);
        builder.HasIndex(image => new { image.ProductId, image.IsMain });
        builder.HasIndex(image => image.StoragePath).IsUnique();

        builder.HasOne(image => image.Product)
            .WithMany(product => product.Images)
            .HasForeignKey(image => image.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
