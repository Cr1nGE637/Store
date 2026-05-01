using CSharpFunctionalExtensions;
using MediatR;
using Store.Ordering.Application.DTOs;
using Store.Ordering.Domain.Interfaces;

namespace Store.Ordering.Application.CQRS.Query;

public class GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderByIdQuery, Result<GetOrderDto>>
{
    public async Task<Result<GetOrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await orderRepository.GetByIdAsync(request.OrderId);
        if (result.IsFailure)
            return Result.Failure<GetOrderDto>(result.Error);

        return Result.Success(MapToDto(result.Value));
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
