using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Command;

public sealed class SetMainProductImageCommand : IRequest<Result<ProductImageDto>>
{
    public Guid ProductId { get; init; }
    public Guid ProductImageId { get; init; }
}
