using CSharpFunctionalExtensions;
using MediatR;
using Users.Application.DTOs;

namespace Users.Application.CQRS.Command;

public class LoginCommand :  IRequest<Result<LoginDto>>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}