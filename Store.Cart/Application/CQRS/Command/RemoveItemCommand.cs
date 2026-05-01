using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;

namespace Store.Carts.Application.CQRS.Command;

public record RemoveItemCommand(
    Guid CustomerId,
    Guid CartItemId) : IRequest<Result<GetCartDto>>;
