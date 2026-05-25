using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Command;

public class DeleteCategoryCommand : IRequest<Result<GetCategoryDto>>
{
    public Guid CategoryId { get; init; }
}
