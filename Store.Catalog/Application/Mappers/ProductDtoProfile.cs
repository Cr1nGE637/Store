using AutoMapper;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Entities;

namespace Store.Catalog.Application.Mappers;

public class ProductDtoProfile : Profile
{
    public ProductDtoProfile()
    {
        CreateMap<Product, CreateProductDto>()
            .ConstructUsing(src => new CreateProductDto(src.ProductName, src.ProductDescription, src.ProductPrice));

        CreateMap<Product, GetProductDto>()
            .ConstructUsing(src => new GetProductDto(src.ProductId, src.ProductName, src.ProductDescription, src.ProductPrice));
    }
}
