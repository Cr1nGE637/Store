using System.ComponentModel;
using AutoMapper;
using Store.Domain.Aggregates;
using Store.Domain.ValueObjects;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Mappers;

public class OrderProfile :  Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderEntity>();
        CreateMap<OrderEntity, Order>()
            .ConvertUsing((src, context) =>
            {
                var orderResult = Order.Create(src.CustomerId);
                if (orderResult.IsFailure)
                {
                    throw new InvalidOperationException(orderResult.Error);
                }
                var order = orderResult.Value;
                if (src.OrderedProducts == null) return order;
                foreach (var product in src.OrderedProducts.Select(p => OrderedProduct.Create(p.ProductId, p.Quantity, p.ProductName, p.Price)))
                {
                    if (product.IsFailure)
                    {
                        throw new InvalidOperationException(product.Error);
                    }
                    order.AddProduct(product.Value);
                }

                return order;
            });
    }
}