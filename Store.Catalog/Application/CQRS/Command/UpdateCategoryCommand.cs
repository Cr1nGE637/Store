using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Command;

public class UpdateCategoryCommand : IRequest<Result<GetCategoryDto>>
{
    [JsonIgnore]
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
}
