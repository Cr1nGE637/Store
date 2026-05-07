using CSharpFunctionalExtensions;
using MediatR;
using Store.Ordering.Application.DTOs;

namespace Store.Ordering.Application.CQRS.Query;

public record GetOrdersByCustomerQuery(Guid CustomerId, int Page = 1, int PageSize = 50)
    : IRequest<Result<IReadOnlyList<GetOrderDto>>>;
