using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;

namespace Store.Carts.Application.CQRS.Command;

public class CheckoutCommand : IRequest<Result<CheckoutResultDto>>
{
    public Guid CustomerId { get; init; }
}
