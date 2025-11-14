using AutoMapper;
using Store.Application.CQRS.Customers.Query;
using Store.Application.DTOs;
using Store.Domain.Entities;

namespace Store.Application.Mappers;

public class CustomerDtoProfile : Profile
{
    public CustomerDtoProfile()
    {
        CreateMap<Customer, GetCustomerDto>()
            .ConstructUsing(src => new GetCustomerDto(src.Id, src.Name, src.Email));
        
        CreateMap<Customer, CreateCustomerDto>()
            .ConstructUsing(src => new CreateCustomerDto(src.Name, src.Email));
    }
}