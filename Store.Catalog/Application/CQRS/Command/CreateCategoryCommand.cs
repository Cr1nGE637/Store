using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Command;

public class CreateCategoryCommand : IRequest<Result<CreateCategoryDto>>
{
    public string CategoryName { get; init; } = string.Empty;
}
