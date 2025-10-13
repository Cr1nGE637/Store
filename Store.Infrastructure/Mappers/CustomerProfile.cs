using AutoMapper;
using Store.Domain.Entities;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Mappers;

public class CustomerProfile :  Profile
{
    public CustomerProfile()
    {
        CreateMap<CustomerEntity, Customer>()
            .ConstructUsing(src => Customer.Create(src.Id, src.Name, src.Email));
        CreateMap<Customer, CustomerEntity>();
    }
}