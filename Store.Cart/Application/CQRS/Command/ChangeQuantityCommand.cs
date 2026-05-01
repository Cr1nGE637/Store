using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;

namespace Store.Carts.Application.CQRS.Command;

public record ChangeQuantityCommand(
    Guid CustomerId,
    Guid CartItemId,
    int Quantity) : IRequest<Result<GetCartDto>>;
