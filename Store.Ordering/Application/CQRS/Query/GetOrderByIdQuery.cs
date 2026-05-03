using CSharpFunctionalExtensions;
using MediatR;
using Store.Ordering.Application.DTOs;

namespace Store.Ordering.Application.CQRS.Query;

public record GetOrderByIdQuery(Guid OrderId, Guid RequesterId, bool IsManager) : IRequest<Result<GetOrderDto>>;
