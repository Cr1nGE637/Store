using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;

namespace Store.Carts.Application.CQRS.Command;

public record AddItemCommand(
    Guid CustomerId,
    Guid ProductId,
    int Quantity) : IRequest<Result<GetCartDto>>;
