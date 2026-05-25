using CSharpFunctionalExtensions;
using Store.Identity.Application.DTOs;
using MediatR;

namespace Store.Identity.Application.CQRS.Query;

public class GetUserQuery : IRequest<Result<GetUserDto>>
{
    public required string Email { get; init; }
}