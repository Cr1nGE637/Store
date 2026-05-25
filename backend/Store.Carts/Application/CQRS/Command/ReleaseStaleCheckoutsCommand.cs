using CSharpFunctionalExtensions;
using MediatR;
using Store.Carts.Application.DTOs;

namespace Store.Carts.Application.CQRS.Command;

public class ReleaseStaleCheckoutsCommand : IRequest<Result<ReleaseStaleCheckoutsResultDto>>
{
    public DateTimeOffset StaleBefore { get; init; }
}
