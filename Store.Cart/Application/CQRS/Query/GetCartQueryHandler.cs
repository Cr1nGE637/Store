using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;
using Store.Carts.Domain.Aggregates;
using Store.Carts.Domain.Interfaces;

namespace Store.Carts.Application.CQRS.Query;

public class GetCartQueryHandler(ICartRepository cartRepository) : IRequestHandler<GetCartQuery, Result<GetCartDto>>
{
    public async Task<Result<GetCartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cartResult = await cartRepository.GetByCustomerIdAsync(request.CustomerId);
        if (cartResult.IsFailure)
            return Result.Failure<GetCartDto>(cartResult.Error);

        return Result.Success(MapToDto(cartResult.Value));
    }

    private static GetCartDto MapToDto(Cart cart) => new(
        cart.CartId,
        cart.CustomerId,
        cart.Items
            .Select(i => new CartItemDto(i.CartItemId, i.ProductId, i.ProductName, i.Price, i.Quantity))
            .ToList());
}
