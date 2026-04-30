using CSharpFunctionalExtensions;
using MediatR;
using Users.Application.DTOs;

namespace Users.Application.CQRS.Query;

public class GetUserQuery : IRequest<Result<GetUserDto>>
{
    public required string Email { get; init; }
}