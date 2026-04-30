using AutoMapper;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Infrastructure.Entity;

namespace Store.Catalog.Infrastructure.Mappers;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<ProductEntity, Product>()
            .ConstructUsing(src => Product.Create(src.ProductName, src.ProductDescription, src.ProductPrice, src.ProductId).Value);

        CreateMap<Product, ProductEntity>();
    }
}