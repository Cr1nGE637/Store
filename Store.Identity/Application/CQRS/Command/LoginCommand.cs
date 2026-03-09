using CSharpFunctionalExtensions;
using MediatR;
using Users.Application.DTOs;

namespace Users.Application.CQRS.Command;

public class LoginCommand :  IRequest<Result<LoginDto>>
{
    public string Email { get; init; }
    public string Password { get; init; }
}