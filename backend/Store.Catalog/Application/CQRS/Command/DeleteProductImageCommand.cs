using CSharpFunctionalExtensions;
using MediatR;

namespace Store.Catalog.Application.CQRS.Command;

public sealed class DeleteProductImageCommand : IRequest<Result>
{
    public Guid ProductId { get; init; }
    public Guid ProductImageId { get; init; }
}
