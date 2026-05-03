using CSharpFunctionalExtensions;
using MediatR;
using Store.Inventory.Application.DTOs;
using Store.Inventory.Domain.Interfaces;

namespace Store.Inventory.Application.CQRS.Query;

public class GetStockQueryHandler(IStockItemRepository repository) : IRequestHandler<GetStockQuery, Result<StockDto>>
{
    public async Task<Result<StockDto>> Handle(GetStockQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.GetByProductIdAsync(request.ProductId);
        if (result.IsFailure)
            return Result.Failure<StockDto>(result.Error);

        return Result.Success(InventoryMappings.ToDto(result.Value));
    }
}
