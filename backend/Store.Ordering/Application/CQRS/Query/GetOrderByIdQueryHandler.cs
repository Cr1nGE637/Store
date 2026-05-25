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

        var order = result.Value;
        if (!request.IsManager && order.CustomerId != request.RequesterId)
            return Result.Failure<GetOrderDto>("Access denied");

        return Result.Success(OrderingMappings.ToGetOrderDto(order));
    }
}
