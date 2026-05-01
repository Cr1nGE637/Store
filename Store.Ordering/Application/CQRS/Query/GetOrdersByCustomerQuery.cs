using CSharpFunctionalExtensions;
using MediatR;
using Store.Ordering.Application.DTOs;

namespace Store.Ordering.Application.CQRS.Query;

public record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<GetOrderDto>>>;
