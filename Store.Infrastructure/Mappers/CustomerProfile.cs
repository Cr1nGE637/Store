using AutoMapper;
using Store.Domain.Entities;
using Store.Domain.ValueObjects;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Mappers;

public class CustomerProfile :  Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerEntity>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => src.Email.ToString()));
        
        CreateMap<CustomerEntity, Customer>()
            .ForMember(dest => dest.Id, 
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => 
                    Email.Create(src.Email).Value));
    }
}