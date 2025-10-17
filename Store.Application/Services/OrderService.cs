using CSharpFunctionalExtensions;
using Store.Application.DTOs;
using Store.Domain.Aggregates;
using Store.Domain.Interfaces;
using Store.Domain.ValueObjects;

namespace Store.Application.Services;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    public async Task<Result<Order>> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        return order.IsSuccess ? Result.Success(order.Value) : Result.Failure<Order>(order.Error);
    }

    public async Task<Result> CreateOrderAsync(Guid customerId, List<OrderProductRequest> orderedProduct)
    {
        var order = Order.Create(customerId);
        if (order.IsFailure)
        {
            return Result.Failure(order.Error);
        }
        foreach (var product in orderedProduct)
        {
            var productResult = OrderedProduct.Create(
                product.ProductId,
                product.ProductQuantity,
                product.ProductName,
                product.ProductPrice
            );

            if (productResult.IsFailure)
            {
                return Result.Failure(productResult.Error);
            }
            
            order.Value.AddProduct(productResult.Value);
        }
        var result = await _orderRepository.CreateAsync(order.Value);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error); 
    }

    public async Task<Result> UpdateOrderAsync(Order order)
    {
        var result = await _orderRepository.UpdateAsync(order);
        return result.IsSuccess ? Result.Success() : Result.Failure<Order>(result.Error);
    }

    public async Task<Result> DeleteOrderAsync(Order order)
    {
        var result = await _orderRepository.DeleteAsync(order);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }
}