using CSharpFunctionalExtensions;
using MediatR;

namespace Store.Inventory.Application.CQRS.Command;

public record ReplenishStockCommand(Guid ProductId, int Amount) : IRequest<Result>;
