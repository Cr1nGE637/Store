using CSharpFunctionalExtensions;
using Store.Application.DTOs;
using Store.Domain.Aggregates;

namespace Store.Application.Interfaces;

public interface IOrderService
{
    Task<Result<Order>> GetOrderByIdAsync(Guid orderId);
    Task<Result> CreateOrderAsync(Guid customerId, List<OrderProductDto> orderedProducts);
    Task<Result> UpdateOrderAsync(Guid orderId, OrderProductDto orderedProduct);
    Task<Result> DeleteOrderAsync(Order order);
}