using AutoMapper;
using CSharpFunctionalExtensions;
using Store.Domain.ValueObjects;
using Store.Infrastructure.Entities;

namespace Store.Infrastructure.Mappers;

public class OrderedProductProfile :  Profile
{
    public OrderedProductProfile()
    {
        CreateMap<OrderedProduct, OrderedProductEntity>();
        CreateMap<OrderedProductEntity, OrderedProduct>()
            .ConstructUsing((src, context) =>
            {
                var result = OrderedProduct.Create(
                    productId: src.ProductId,
                    quantity: src.Quantity,
                    productName: src.ProductName,
                    price: src.Price
                );

                if (result.IsFailure)
                {
                    Result.Failure($"Cannot restore OrderedProduct from database: {result.Error}");
                }

                return result.Value;
            });
    }
}