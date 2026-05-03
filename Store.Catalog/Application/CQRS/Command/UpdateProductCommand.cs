using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Command;

public class UpdateProductCommand : IRequest<Result<GetProductDto>>
{
    [JsonIgnore]
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductDescription { get; init; } = string.Empty;
    public decimal ProductPrice { get; init; }
    public Guid CategoryId { get; init; }
}
