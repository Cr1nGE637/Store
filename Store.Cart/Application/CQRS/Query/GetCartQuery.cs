using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;

namespace Store.Carts.Application.CQRS.Query;

public record GetCartQuery(Guid CustomerId) : IRequest<Result<GetCartDto>>;
