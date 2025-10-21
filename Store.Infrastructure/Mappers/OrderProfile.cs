using System.ComponentModel;
using AutoMapper;
using CSharpFunctionalExtensions;
using Store.Domain.Aggregates;
using Store.Domain.Enums;
using Store.Domain.ValueObjects;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Mappers;

public class OrderProfile :  Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderEntity>();
        CreateMap<OrderEntity, Order>()
            .ConstructUsing((src, context) =>
            {
                var products = context.Mapper.Map<List<OrderedProduct>>(src.OrderedProducts ?? new List<OrderedProductEntity>());

                return Order.Restore(orderId: src.Id, customerId: src.CustomerId, orderDate: src.OrderDate, status: src.Status, totalPrice: src.TotalPrice, products: products).Value;
            });

    }
}