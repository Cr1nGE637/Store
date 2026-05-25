using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application;
using Store.Carts.Application.DTOs;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Aggregates;
using Store.Carts.Domain.Interfaces;

namespace Store.Carts.Application.CQRS.Query;

public class GetCartQueryHandler(
    ICartRepository cartRepository,
    ICartUnitOfWork unitOfWork) : IRequestHandler<GetCartQuery, Result<GetCartDto>>
{
    public async Task<Result<GetCartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        using var customerCartLock = await CustomerCartMutationLocks.AcquireAsync(request.CustomerId, cancellationToken);

        var cartResult = await cartRepository.GetByCustomerIdAsync(request.CustomerId);
        if (cartResult.IsSuccess)
            return Result.Success(CartMappings.ToGetCartDto(cartResult.Value));

        if (cartResult.Error != "Cart not found")
            return Result.Failure<GetCartDto>(cartResult.Error);

        var createResult = Cart.Create(request.CustomerId);
        if (createResult.IsFailure)
            return Result.Failure<GetCartDto>(createResult.Error);

        var addResult = await cartRepository.AddAsync(createResult.Value);
        if (addResult.IsFailure)
            return Result.Failure<GetCartDto>(addResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(CartMappings.ToGetCartDto(createResult.Value));
    }
}
