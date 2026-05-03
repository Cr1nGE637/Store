using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;

namespace Store.Carts.Application.CQRS.Query;

public class GetCartQuery : IRequest<Result<GetCartDto>>
{
    public Guid CustomerId { get; init; }
}
