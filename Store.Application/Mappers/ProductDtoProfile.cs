using AutoMapper;
using Store.Application.DTOs;
using Store.Domain.Entities;

namespace Store.Application.Mappers;

public class ProductDtoProfile : Profile
{
    public ProductDtoProfile()
    {
        CreateMap<Product, CreateProductDto>()
            .ConstructUsing(src => new CreateProductDto(src.Name, src.Description, src.Price));
        
        CreateMap<Product, GetProductDto>()
            .ConstructUsing(src => new GetProductDto(src.Id, src.Name, src.Description, src.Price));
    }
}