using CSharpFunctionalExtensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Store.Catalog.Application.Interfaces;

namespace Store.Catalog.Infrastructure.Services;

public sealed class LocalProductImageStorage(
    IHostEnvironment environment,
    IOptions<ProductImageStorageOptions> options)
    : IProductImageStorage
{
    private static readonly IReadOnlyDictionary<string, string> ExtensionsByContentType =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp"
        };

    public async Task<Result<ProductImageFileResult>> SaveAsync(
        Guid productId,
        Stream content,
        string originalFileName,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken)
    {
        var validation = Validate(productId, content, originalFileName, contentType, sizeBytes);
        if (validation.IsFailure)
            return Result.Failure<ProductImageFileResult>(validation.Error);

        var extension = ExtensionsByContentType[contentType];
        var imageId = Guid.NewGuid();
        var safeOriginalFileName = Path.GetFileName(originalFileName.Trim());
        var rootPath = GetRootPath();
        var productDirectory = Path.Combine(rootPath, productId.ToString("N"));
        Directory.CreateDirectory(productDirectory);

        var fileName = $"{imageId:N}{extension}";
        var filePath = Path.Combine(productDirectory, fileName);
        await using (var file = File.Create(filePath))
        {
            await content.CopyToAsync(file, cancellationToken);
        }

        var storagePath = ToStoragePath(rootPath, filePath);
        var url = $"{options.Value.PublicBasePath.TrimEnd('/')}/{productId:N}/{fileName}";
        return Result.Success(new ProductImageFileResult(
            url,
            storagePath,
            safeOriginalFileName,
            contentType,
            sizeBytes));
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            return Task.CompletedTask;

        var rootPath = GetRootPath();
        var filePath = Path.GetFullPath(Path.Combine(rootPath, storagePath));
        if (!IsUnderRoot(rootPath, filePath))
            return Task.CompletedTask;

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    private Result Validate(
        Guid productId,
        Stream content,
        string originalFileName,
        string contentType,
        long sizeBytes)
    {
        if (productId == Guid.Empty)
            return Result.Failure("Product is required");

        if (content == Stream.Null || !content.CanRead)
            return Result.Failure("Image file is required");

        if (string.IsNullOrWhiteSpace(originalFileName))
            return Result.Failure("Image file name is required");

        if (string.IsNullOrWhiteSpace(contentType))
            return Result.Failure("Image content type is required");

        if (sizeBytes <= 0)
            return Result.Failure("Image file cannot be empty");

        if (sizeBytes > options.Value.MaxFileSizeBytes)
            return Result.Failure($"Image file cannot exceed {options.Value.MaxFileSizeBytes} bytes");

        if (!options.Value.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return Result.Failure("Image content type is not supported");

        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension)
            || !string.Equals(extension, ExtensionsByContentType[contentType], StringComparison.OrdinalIgnoreCase)
            && !(string.Equals(contentType, "image/jpeg", StringComparison.OrdinalIgnoreCase)
                && string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)))
            return Result.Failure("Image file extension does not match content type");

        return Result.Success();
    }

    private string GetRootPath()
    {
        var configuredRoot = options.Value.RootPath;
        var rootPath = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot);

        return Path.GetFullPath(rootPath);
    }

    private static string ToStoragePath(string rootPath, string filePath)
    {
        var relative = Path.GetRelativePath(rootPath, filePath);
        return relative.Replace(Path.DirectorySeparatorChar, '/');
    }

    private static bool IsUnderRoot(string rootPath, string filePath)
    {
        var normalizedRoot = rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? rootPath
            : rootPath + Path.DirectorySeparatorChar;

        return filePath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }
}
