using Microsoft.AspNetCore.CookiePolicy;
using Store.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Store.App.Extensions;
using Store.Catalog;
using Store.Catalog.Infrastructure.DbContext;
using Store.Domain.Interfaces;
using Store.Infrastructure.Repositories;
using Users;
using Users.Infrastructure.DbContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnectionString")));

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);

builder.Services.AddScoped<ICustomersRepository, CustomersRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

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