using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;

namespace Store.Carts.Application.CQRS.Command;

public class ChangeQuantityCommand : IRequest<Result<GetCartDto>>
{
    public Guid CustomerId { get; init; }
    public Guid CartItemId { get; init; }
    public int Quantity { get; init; }
}
