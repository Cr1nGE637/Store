using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Aggregates;
using Store.Carts.Domain.Interfaces;

namespace Store.Carts.Application.CQRS.Command;

public class ChangeQuantityCommandHandler(
    ICartRepository cartRepository,
    ICartUnitOfWork unitOfWork) : IRequestHandler<ChangeQuantityCommand, Result<GetCartDto>>
{
    public async Task<Result<GetCartDto>> Handle(ChangeQuantityCommand request, CancellationToken cancellationToken)
    {
        var cartResult = await cartRepository.GetByCustomerIdAsync(request.CustomerId);
        if (cartResult.IsFailure)
            return Result.Failure<GetCartDto>(cartResult.Error);

        var cart = cartResult.Value;

        var changeResult = cart.ChangeQuantity(request.CartItemId, request.Quantity);
        if (changeResult.IsFailure)
            return Result.Failure<GetCartDto>(changeResult.Error);

        var updateResult = await cartRepository.UpdateAsync(cart);
        if (updateResult.IsFailure)
            return Result.Failure<GetCartDto>(updateResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(cart));
    }

    private static GetCartDto MapToDto(Cart cart) => new(
        cart.CartId,
        cart.CustomerId,
        cart.Items
            .Select(i => new CartItemDto(i.CartItemId, i.ProductId, i.ProductName, i.Price, i.Quantity))
            .ToList());
}
