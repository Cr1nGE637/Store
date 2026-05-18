using Store.Identity;
using Store.Identity.Infrastructure.DbContexts;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using Store.App.Extensions;
using Store.Catalog;
using Store.Catalog.Infrastructure.DbContexts;
using Store.Carts;
using Store.Carts.Infrastructure.DbContexts;
using Store.Ordering;
using Store.Ordering.Infrastructure.DbContexts;
using Store.Inventory;
using Store.Inventory.Infrastructure.DbContexts;
using Store.Notifications;
using Store.Notifications.Infrastructure.DbContexts;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerWithBearer();
builder.Services.AddConfiguredCors(builder.Configuration);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddJwtAuthentication();
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddCartModule(builder.Configuration);
builder.Services.AddOrderingModule(builder.Configuration);
builder.Services.AddInventoryModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
}

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    Secure = CookieSecurePolicy.SameAsRequest,
    HttpOnly = HttpOnlyPolicy.Always
});

app.UseStoreRequestLogging();
app.UseRateLimiter();
app.UseCors(ApiExtensions.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await services.ApplyMigrationsAsync<IdentityDbContext>();
    await services.ApplyMigrationsAsync<CatalogDbContext>();
    await services.ApplyMigrationsAsync<CartDbContext>();
    await services.ApplyMigrationsAsync<OrderingDbContext>();
    await services.ApplyMigrationsAsync<InventoryDbContext>();
    await services.ApplyMigrationsAsync<NotificationsDbContext>();
    await services.SeedDemoElectronicsAsync();
    await services.SyncCartProductCacheAsync();
}

app.Run();
