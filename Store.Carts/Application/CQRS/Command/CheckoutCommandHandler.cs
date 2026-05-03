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
    IPublisher publisher) : IRequestHandler<CheckoutCommand, Result<CheckoutResultDto>>
{
    public async Task<Result<CheckoutResultDto>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var cartResult = await cartRepository.GetByCustomerIdAsync(request.CustomerId);
        if (cartResult.IsFailure)
            return Result.Failure<CheckoutResultDto>(cartResult.Error);

        var cart = cartResult.Value;

        var checkoutResult = cart.Checkout();
        if (checkoutResult.IsFailure)
            return Result.Failure<CheckoutResultDto>(checkoutResult.Error);

        var items = checkoutResult.Value;

        var updateResult = await cartRepository.UpdateAsync(cart);
        if (updateResult.IsFailure)
            return Result.Failure<CheckoutResultDto>(updateResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in cart.DomainEvents)
            await publisher.Publish(domainEvent, cancellationToken);
        cart.ClearDomainEvents();

        return Result.Success(CartMappings.ToCheckoutResultDto(cart, items));
    }
}
