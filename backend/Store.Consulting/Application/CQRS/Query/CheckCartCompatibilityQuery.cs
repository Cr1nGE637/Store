using CSharpFunctionalExtensions;
using MediatR;
using Store.Consulting.Application.DTOs;

namespace Store.Consulting.Application.CQRS.Query;

public sealed class CheckCartCompatibilityQuery : IRequest<Result<ConsultationResultDto>>
{
    public Guid? CustomerId { get; init; }
    public IReadOnlyCollection<Guid> ProductIds { get; init; } = [];
}
