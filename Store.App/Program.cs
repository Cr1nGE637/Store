using Identity;
using Identity.Infrastructure.DbContexts;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using Store.App.Extensions;
using Store.Catalog;
using Store.Catalog.Infrastructure.DbContexts;
using Store.Carts;
using Store.Carts.Infrastructure.DbContexts;
using Store.Ordering;
using Store.Ordering.Infrastructure.DbContexts;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerWithBearer();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddCartModule(builder.Configuration);
builder.Services.AddOrderingModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    await services.ApplyMigrationsAsync<IdentityDbContext>();
    await services.ApplyMigrationsAsync<CatalogDbContext>();
    await services.ApplyMigrationsAsync<CartDbContext>();
    await services.ApplyMigrationsAsync<OrderingDbContext>();
}

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    Secure = CookieSecurePolicy.Always,
    HttpOnly = HttpOnlyPolicy.Always
});

app.UseAuthentication();
app.UseAuthorization();

app.Run();