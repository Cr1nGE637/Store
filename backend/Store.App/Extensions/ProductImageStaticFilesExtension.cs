using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Store.Catalog.Infrastructure.Services;

namespace Store.App.Extensions;

public static class ProductImageStaticFilesExtension
{
    public static WebApplication UseProductImageStaticFiles(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<ProductImageStorageOptions>>().Value;
        if (string.IsNullOrWhiteSpace(options.RootPath) || string.IsNullOrWhiteSpace(options.PublicBasePath))
            return app;

        var rootPath = Path.IsPathRooted(options.RootPath)
            ? options.RootPath
            : Path.Combine(app.Environment.ContentRootPath, options.RootPath);

        rootPath = Path.GetFullPath(rootPath);
        Directory.CreateDirectory(rootPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(rootPath),
            RequestPath = "/" + options.PublicBasePath.Trim().Trim('/')
        });

        return app;
    }
}
