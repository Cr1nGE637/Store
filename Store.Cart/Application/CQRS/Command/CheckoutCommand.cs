using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;

namespace Store.Carts.Application.CQRS.Command;

public record CheckoutCommand(Guid CustomerId) : IRequest<Result<CheckoutResultDto>>;
