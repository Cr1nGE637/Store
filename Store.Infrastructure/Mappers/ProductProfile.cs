using AutoMapper;
using Store.Domain.Entities;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Mappers;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<ProductEntity, Product>()
            .ConstructUsing(src => Product.Create(src.Name, src.Description, src.Price).Value);

        CreateMap<Product, ProductEntity>();
    }
}