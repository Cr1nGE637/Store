using CSharpFunctionalExtensions;
using Store.Application.DTOs;
using Store.Domain.Aggregates;
using Store.Domain.Interfaces;
using Store.Domain.ValueObjects;

namespace Store.Application.Services;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductService _productService;
    public OrderService(IOrderRepository orderRepository, IProductService productService)
    {
        _orderRepository = orderRepository;
        _productService = productService;
    }
    public async Task<Result<Order>> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        return order.IsSuccess ? Result.Success(order.Value) : Result.Failure<Order>(order.Error);
    }

    public async Task<Result> CreateOrderAsync(Guid customerId, List<OrderProductRequest> orderedProducts)
    {
        var order = Order.Create(customerId);
        if (order.IsFailure)
        {
            return Result.Failure(order.Error);
        }
        var productResult = await _productService.GetProductById(orderedProducts.First().ProductId);
        if (productResult.IsFailure) return Result.Failure(productResult.Error);
        var product = productResult.Value;
        foreach (var orderProduct in orderedProducts)
        {
            var orderProductResult = OrderedProduct.Create(
                orderProduct.ProductId,
                orderProduct.ProductQuantity,
                product.Name,
                product.Price
            );

            if (orderProductResult.IsFailure)
            {
                return Result.Failure(orderProductResult.Error);
            }
            
            order.Value.AddProduct(orderProductResult.Value);
        }
        var result = await _orderRepository.CreateAsync(order.Value);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error); 
    }

    public async Task<Result> UpdateOrderAsync(Guid orderId, OrderProductRequest orderedProduct)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order.IsFailure)
        {
            return Result.Failure(order.Error);
        }
        var productResult = await _productService.GetProductById(orderedProduct.ProductId);
        if (productResult.IsFailure) return Result.Failure(productResult.Error);
        var product = productResult.Value;
        order.Value.UpdateProduct(orderedProduct.ProductId, orderedProduct.ProductQuantity, product.Name, product.Price);
        var result = await _orderRepository.UpdateAsync(order.Value);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }

    public async Task<Result> DeleteOrderAsync(Order order)
    {
        var result = await _orderRepository.DeleteAsync(order);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }
}