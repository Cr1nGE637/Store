using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Infrastructure.DbContexts;
using Store.Catalog.Infrastructure.Entity;

namespace Store.Catalog.Infrastructure.Repository;

public sealed class ProductImageRepository(CatalogDbContext dbContext) : IProductImageRepository
{
    public async Task AddAsync(ProductImage image, CancellationToken cancellationToken)
    {
        await dbContext.ProductImages.AddAsync(ToEntity(image), cancellationToken);
    }

    public async Task ClearMainImageAsync(Guid productId, CancellationToken cancellationToken)
    {
        var images = await dbContext.ProductImages
            .Where(image => image.ProductId == productId && image.IsMain)
            .ToListAsync(cancellationToken);

        foreach (var image in images)
        {
            image.IsMain = false;
        }
    }

    public async Task<int> GetNextDisplayOrderAsync(Guid productId, CancellationToken cancellationToken)
    {
        var currentMax = await dbContext.ProductImages
            .Where(image => image.ProductId == productId)
            .Select(image => (int?)image.DisplayOrder)
            .MaxAsync(cancellationToken);

        return (currentMax ?? -1) + 1;
    }

    public async Task<Result<ProductImageDto>> GetByIdAsync(Guid productImageId, CancellationToken cancellationToken)
    {
        var image = await dbContext.ProductImages
            .AsNoTracking()
            .FirstOrDefaultAsync(image => image.ProductImageId == productImageId, cancellationToken);

        return image is null
            ? Result.Failure<ProductImageDto>("Product image not found")
            : Result.Success(ToDto(image));
    }

    public async Task<IReadOnlyCollection<ProductImageDto>> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var images = await dbContext.ProductImages
            .AsNoTracking()
            .Where(image => image.ProductId == productId)
            .OrderByDescending(image => image.IsMain)
            .ThenBy(image => image.DisplayOrder)
            .ToListAsync(cancellationToken);

        return images.Select(ToDto).ToArray();
    }

    public async Task<Result<ProductImageDto>> DeleteAsync(
        Guid productId,
        Guid productImageId,
        CancellationToken cancellationToken)
    {
        var image = await dbContext.ProductImages
            .FirstOrDefaultAsync(
                image => image.ProductId == productId && image.ProductImageId == productImageId,
                cancellationToken);

        if (image is null)
            return Result.Failure<ProductImageDto>("Product image not found");

        var dto = ToDto(image);
        dbContext.ProductImages.Remove(image);
        return Result.Success(dto);
    }

    public async Task<Result<ProductImageDto>> SetMainAsync(
        Guid productId,
        Guid productImageId,
        CancellationToken cancellationToken)
    {
        var image = await dbContext.ProductImages
            .FirstOrDefaultAsync(
                image => image.ProductId == productId && image.ProductImageId == productImageId,
                cancellationToken);

        if (image is null)
            return Result.Failure<ProductImageDto>("Product image not found");

        await ClearMainImageAsync(productId, cancellationToken);
        image.IsMain = true;
        return Result.Success(ToDto(image));
    }

    public async Task<Result<ProductImageDto?>> SetFirstAvailableAsMainAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var image = await dbContext.ProductImages
            .Where(image => image.ProductId == productId)
            .OrderBy(image => image.DisplayOrder)
            .FirstOrDefaultAsync(cancellationToken);

        if (image is null)
            return Result.Success<ProductImageDto?>(null);

        await ClearMainImageAsync(productId, cancellationToken);
        image.IsMain = true;
        return Result.Success<ProductImageDto?>(ToDto(image));
    }

    public async Task<IReadOnlyDictionary<Guid, ProductImageDto>> GetMainImagesAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
            return new Dictionary<Guid, ProductImageDto>();

        var images = await dbContext.ProductImages
            .AsNoTracking()
            .Where(image => productIds.Contains(image.ProductId))
            .OrderByDescending(image => image.IsMain)
            .ThenBy(image => image.DisplayOrder)
            .ToListAsync(cancellationToken);

        return images
            .GroupBy(image => image.ProductId)
            .ToDictionary(group => group.Key, group => ToDto(group.First()));
    }

    private static ProductImageEntity ToEntity(ProductImage image) => new()
    {
        ProductImageId = image.ProductImageId,
        ProductId = image.ProductId,
        Url = image.Url,
        StoragePath = image.StoragePath,
        OriginalFileName = image.OriginalFileName,
        ContentType = image.ContentType,
        SizeBytes = image.SizeBytes,
        AltText = image.AltText,
        IsMain = image.IsMain,
        DisplayOrder = image.DisplayOrder,
        CreatedAtUtc = image.CreatedAtUtc
    };

    private static ProductImageDto ToDto(ProductImageEntity image) =>
        new(
            image.ProductImageId,
            image.ProductId,
            image.Url,
            image.StoragePath,
            image.OriginalFileName,
            image.ContentType,
            image.SizeBytes,
            image.AltText,
            image.IsMain,
            image.DisplayOrder,
            image.CreatedAtUtc);
}
