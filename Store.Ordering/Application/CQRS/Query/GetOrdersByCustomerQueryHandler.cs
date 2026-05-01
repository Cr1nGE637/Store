using CSharpFunctionalExtensions;
using MediatR;
using Store.Ordering.Application.DTOs;
using Store.Ordering.Domain.Interfaces;

namespace Store.Ordering.Application.CQRS.Query;

public class GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrdersByCustomerQuery, Result<IReadOnlyList<GetOrderDto>>>
{
    public async Task<Result<IReadOnlyList<GetOrderDto>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByCustomerIdAsync(request.CustomerId);
        var dtos = orders.Select(MapToDto).ToList();
        return Result.Success<IReadOnlyList<GetOrderDto>>(dtos);
    }

    private static GetOrderDto MapToDto(Domain.Aggregates.Order order) => new(
        order.OrderId,
        order.CustomerId,
        order.Status.ToString(),
        order.CreatedAt,
        order.PaidAt,
        order.CancelledAt,
        order.Products.Select(p => new OrderedProductDto(p.ProductId, p.ProductName, p.Price, p.Quantity)).ToList());
}
