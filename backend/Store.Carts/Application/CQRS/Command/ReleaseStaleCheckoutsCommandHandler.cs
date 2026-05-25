using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;
using Store.Carts.Application.Interfaces;
using Store.Carts.Domain.Interfaces;

namespace Store.Carts.Application.CQRS.Command;

public class ReleaseStaleCheckoutsCommandHandler(
    ICartRepository cartRepository,
    ICheckoutOrderLookup checkoutOrderLookup,
    ICartUnitOfWork unitOfWork) : IRequestHandler<ReleaseStaleCheckoutsCommand, Result<ReleaseStaleCheckoutsResultDto>>
{
    public async Task<Result<ReleaseStaleCheckoutsResultDto>> Handle(
        ReleaseStaleCheckoutsCommand request,
        CancellationToken cancellationToken)
    {
        var carts = await cartRepository.GetPendingCheckoutsStartedBeforeAsync(request.StaleBefore);

        var released = 0;
        var skippedBecauseOrderExists = 0;

        foreach (var cart in carts)
        {
            if (cart.PendingCheckoutId == null)
                return Result.Failure<ReleaseStaleCheckoutsResultDto>("Pending checkout id is missing");

            var orderExists = await checkoutOrderLookup.HasOrderForCheckoutAsync(
                cart.PendingCheckoutId.Value,
                cancellationToken);
            if (orderExists)
                skippedBecauseOrderExists++;

            var releaseResult = cart.ReleaseStaleCheckout(request.StaleBefore, orderExists);
            if (releaseResult.IsFailure)
                return Result.Failure<ReleaseStaleCheckoutsResultDto>(releaseResult.Error);

            if (!releaseResult.Value)
                continue;

            var updateResult = await cartRepository.UpdateAsync(cart);
            if (updateResult.IsFailure)
                return Result.Failure<ReleaseStaleCheckoutsResultDto>(updateResult.Error);

            released++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ReleaseStaleCheckoutsResultDto(
            carts.Count,
            released,
            skippedBecauseOrderExists));
    }
}
