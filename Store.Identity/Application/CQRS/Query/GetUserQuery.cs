using CSharpFunctionalExtensions;
using Identity.Application.DTOs;
using MediatR;

namespace Identity.Application.CQRS.Query;

public class GetUserQuery : IRequest<Result<GetUserDto>>
{
    public required string Email { get; init; }
}