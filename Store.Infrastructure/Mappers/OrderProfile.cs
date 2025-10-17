using AutoMapper;
using Store.Domain.Aggregates;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Mappers;

public class OrderProfile :  Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderEntity>();
        CreateMap<OrderEntity, Order>()
            .ConstructUsing(src => Order.Create().Value)
            .AfterMap((src, dest) =>
            {
                foreach (var product in dest.Products)
                {
                    dest.AddProduct(product);
                }
            });
    }
}