using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;

namespace Store.Carts.Application.CQRS.Command;

public class CheckoutCommand : IRequest<Result<CheckoutResultDto>>
{
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string RecipientName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string DeliveryAddress { get; init; } = string.Empty;
    public string DeliveryMethod { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
}
