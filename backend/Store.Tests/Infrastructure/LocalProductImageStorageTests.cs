using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Store.Catalog.Infrastructure.Services;

namespace Store.Tests.Infrastructure;

public class LocalProductImageStorageTests
{
    [Fact]
    public async Task SaveAsync_WhenImageIsValid_WritesFileAndReturnsPublicUrl()
    {
        var root = CreateTempDirectory();
        try
        {
            var storage = CreateStorage(root);
            var productId = Guid.NewGuid();
            await using var content = new MemoryStream([1, 2, 3, 4]);

            var result = await storage.SaveAsync(
                productId,
                content,
                "photo.jpg",
                "image/jpeg",
                content.Length,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.StartsWith($"/uploads/products/{productId:N}/", result.Value.Url);
            Assert.Equal("photo.jpg", result.Value.OriginalFileName);
            Assert.Equal("image/jpeg", result.Value.ContentType);
            Assert.True(File.Exists(Path.Combine(root, result.Value.StoragePath)));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_WhenContentTypeIsUnsupported_ReturnsFailure()
    {
        var root = CreateTempDirectory();
        try
        {
            var storage = CreateStorage(root);
            await using var content = new MemoryStream([1, 2, 3, 4]);

            var result = await storage.SaveAsync(
                Guid.NewGuid(),
                content,
                "photo.gif",
                "image/gif",
                content.Length,
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal("Image content type is not supported", result.Error);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_WhenExtensionDoesNotMatchContentType_ReturnsFailure()
    {
        var root = CreateTempDirectory();
        try
        {
            var storage = CreateStorage(root);
            await using var content = new MemoryStream([1, 2, 3, 4]);

            var result = await storage.SaveAsync(
                Guid.NewGuid(),
                content,
                "photo.png",
                "image/jpeg",
                content.Length,
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal("Image file extension does not match content type", result.Error);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static LocalProductImageStorage CreateStorage(string root) =>
        new(
            new FakeHostEnvironment(root),
            Options.Create(new ProductImageStorageOptions
            {
                RootPath = root,
                PublicBasePath = "/uploads/products",
                MaxFileSizeBytes = 1024
            }));

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"store_product_images_{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class FakeHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Store.Tests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
