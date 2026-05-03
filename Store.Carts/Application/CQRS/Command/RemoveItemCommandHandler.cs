using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Interfaces;

namespace Store.Carts.Application.CQRS.Command;

public class RemoveItemCommandHandler(
    ICartRepository cartRepository,
    ICartUnitOfWork unitOfWork) : IRequestHandler<RemoveItemCommand, Result<GetCartDto>>
{
    public async Task<Result<GetCartDto>> Handle(RemoveItemCommand request, CancellationToken cancellationToken)
    {
        var cartResult = await cartRepository.GetByCustomerIdAsync(request.CustomerId);
        if (cartResult.IsFailure)
            return Result.Failure<GetCartDto>(cartResult.Error);

        var cart = cartResult.Value;

        var removeResult = cart.RemoveItem(request.CartItemId);
        if (removeResult.IsFailure)
            return Result.Failure<GetCartDto>(removeResult.Error);

        var updateResult = await cartRepository.UpdateAsync(cart);
        if (updateResult.IsFailure)
            return Result.Failure<GetCartDto>(updateResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(CartMappings.ToGetCartDto(cart));
    }
}
