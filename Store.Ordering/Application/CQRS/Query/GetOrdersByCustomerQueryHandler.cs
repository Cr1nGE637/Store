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
        var pagination = Pagination.Normalize(request.Page, request.PageSize);
        if (pagination.IsFailure)
            return Result.Failure<IReadOnlyList<GetOrderDto>>(pagination.Error);

        var orders = await orderRepository.GetByCustomerIdAsync(
            request.CustomerId,
            pagination.Value.Skip,
            pagination.Value.Take);
        var dtos = orders.Select(OrderingMappings.ToGetOrderDto).ToList();
        return Result.Success<IReadOnlyList<GetOrderDto>>(dtos);
    }
}
