using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;
using Store.Carts.Domain.Interfaces;

namespace Store.Carts.Application.CQRS.Query;

public class GetCartQueryHandler(ICartRepository cartRepository) : IRequestHandler<GetCartQuery, Result<GetCartDto>>
{
    public async Task<Result<GetCartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cartResult = await cartRepository.GetByCustomerIdAsync(request.CustomerId);
        if (cartResult.IsFailure)
            return Result.Failure<GetCartDto>(cartResult.Error);

        return Result.Success(CartMappings.ToGetCartDto(cartResult.Value));
    }
}
