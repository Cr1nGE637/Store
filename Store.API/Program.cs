using Store.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Store.Application.DTOs;
using Store.Application.Mappers;
using Store.Domain.Interfaces;
using Store.Infrastructure.Mappers;
using Store.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnectionString")));

builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateProductDto).Assembly));

builder.Services.AddAutoMapper(cfg =>
    cfg.AddMaps(typeof(ProductDtoProfile).Assembly),
    typeof(ProductProfile).Assembly);

builder.Services.AddScoped<ICustomersRepository, CustomersRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
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
    var context = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
    context.Database.EnsureCreated();
}

app.Run();