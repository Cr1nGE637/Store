using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Aggregates;
using Store.Carts.Domain.Interfaces;

namespace Store.Carts.Application.CQRS.Command;

public class CheckoutCommandHandler(
    ICartRepository cartRepository,
    ICartUnitOfWork unitOfWork,
    ICartDomainEventOutbox outbox) : IRequestHandler<CheckoutCommand, Result<CheckoutResultDto>>
{
    public async Task<Result<CheckoutResultDto>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var cartResult = await cartRepository.GetByCustomerIdAsync(request.CustomerId);
        if (cartResult.IsFailure)
            return Result.Failure<CheckoutResultDto>(cartResult.Error);

        var cart = cartResult.Value;

        var checkoutResult = cart.Checkout(request.CustomerEmail);
        if (checkoutResult.IsFailure)
            return Result.Failure<CheckoutResultDto>(checkoutResult.Error);

        var items = checkoutResult.Value;

        var updateResult = await cartRepository.UpdateAsync(cart);
        if (updateResult.IsFailure)
            return Result.Failure<CheckoutResultDto>(updateResult.Error);

        await outbox.AddAsync(cart.DomainEvents, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        cart.ClearDomainEvents();

        return Result.Success(CartMappings.ToCheckoutResultDto(cart, items));
    }
}
