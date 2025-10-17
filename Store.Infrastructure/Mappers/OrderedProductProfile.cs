using AutoMapper;
using Store.Domain.ValueObjects;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Mappers;

public class OrderedProductProfile :  Profile
{
    public OrderedProductProfile()
    {
        CreateMap<OrderedProduct, OrderedProductEntity>();
        CreateMap<OrderedProductEntity, OrderedProduct>()
            .ConstructUsing(src => OrderedProduct.Create(src.ProductId, src.Quantity, src.ProductName, src.Price).Value);
    }
}