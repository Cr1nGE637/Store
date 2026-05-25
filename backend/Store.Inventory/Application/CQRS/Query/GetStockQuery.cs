using CSharpFunctionalExtensions;
using MediatR;
using Store.Inventory.Application.DTOs;

namespace Store.Inventory.Application.CQRS.Query;

public record GetStockQuery(Guid ProductId) : IRequest<Result<StockDto>>;
