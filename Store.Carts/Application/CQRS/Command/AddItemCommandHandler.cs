using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Aggregates;
using Store.Carts.Domain.Interfaces;

namespace Store.Carts.Application.CQRS.Command;

public class AddItemCommandHandler(
    ICartRepository cartRepository,
    IProductCacheRepository productCacheRepository,
    ICartUnitOfWork unitOfWork) : IRequestHandler<AddItemCommand, Result<GetCartDto>>
{
    public async Task<Result<GetCartDto>> Handle(AddItemCommand request, CancellationToken cancellationToken)
    {
        var productResult = await productCacheRepository.GetByIdAsync(request.ProductId);
        if (productResult.IsFailure)
            return Result.Failure<GetCartDto>("Product not found or unavailable");

        var product = productResult.Value;

        var cartResult = await cartRepository.GetByCustomerIdAsync(request.CustomerId);
        bool isNew = cartResult.IsFailure;

        Cart cart;
        if (isNew)
        {
            var createResult = Cart.Create(request.CustomerId);
            if (createResult.IsFailure)
                return Result.Failure<GetCartDto>(createResult.Error);
            cart = createResult.Value;
        }
        else
        {
            cart = cartResult.Value;
        }

        var addResult = cart.AddItem(product.ProductId, product.ProductName, product.Price, request.Quantity);
        if (addResult.IsFailure)
            return Result.Failure<GetCartDto>(addResult.Error);

        var saveResult = isNew
            ? await cartRepository.AddAsync(cart)
            : await cartRepository.UpdateAsync(cart);

        if (saveResult.IsFailure)
            return Result.Failure<GetCartDto>(saveResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(CartMappings.ToGetCartDto(cart));
    }
}
