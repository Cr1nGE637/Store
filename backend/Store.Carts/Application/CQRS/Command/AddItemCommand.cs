using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;

namespace Store.Carts.Application.CQRS.Command;

public class AddItemCommand : IRequest<Result<GetCartDto>>
{
    public Guid CustomerId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
