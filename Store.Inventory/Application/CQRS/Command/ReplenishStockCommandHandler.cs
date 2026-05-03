using CSharpFunctionalExtensions;
using MediatR;
using Store.Inventory.Application.Interfaces;
using Store.Inventory.Domain.Aggregates;
using Store.Inventory.Domain.Interfaces;

namespace Store.Inventory.Application.CQRS.Command;

public class ReplenishStockCommandHandler(
    IStockItemRepository repository,
    IInventoryUnitOfWork unitOfWork) : IRequestHandler<ReplenishStockCommand, Result>
{
    public async Task<Result> Handle(ReplenishStockCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            return Result.Failure("Amount must be positive");

        var existing = await repository.GetByProductIdAsync(request.ProductId);

        if (existing.IsFailure)
        {
            var createResult = StockItem.Create(request.ProductId, request.Amount);
            if (createResult.IsFailure)
                return Result.Failure(createResult.Error);

            var addResult = await repository.AddAsync(createResult.Value);
            if (addResult.IsFailure)
                return addResult;
        }
        else
        {
            var replenishResult = existing.Value.Replenish(request.Amount);
            if (replenishResult.IsFailure)
                return replenishResult;

            var updateResult = await repository.UpdateAsync(existing.Value);
            if (updateResult.IsFailure)
                return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
