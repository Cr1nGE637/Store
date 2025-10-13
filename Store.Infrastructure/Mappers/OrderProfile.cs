using AutoMapper;
using Store.Domain.Aggregates;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Mappers;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<OrderEntity, Order>()
            .ForMember(src => src.OrderId, dest => dest.MapFrom(src => src.Id));
    }
}