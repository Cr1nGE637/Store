using CSharpFunctionalExtensions;
using Identity.Application.DTOs;
using MediatR;

namespace Identity.Application.CQRS.Command;

public class LoginCommand :  IRequest<Result<LoginDto>>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}