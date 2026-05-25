using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Command;

public class UpdateProductCommand : IRequest<Result<GetProductDto>>
{
    public Guid ProductId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string ProductDescription { get; init; } = string.Empty;
    public decimal ProductPrice { get; init; }
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int WarrantyMonths { get; init; }
    public Guid CategoryId { get; init; }
    public Dictionary<string, string> Specifications { get; init; } = [];
}
