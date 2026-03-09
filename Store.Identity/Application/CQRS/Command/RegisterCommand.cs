using CSharpFunctionalExtensions;
using MediatR;
using Users.Application.DTOs;

namespace Users.Application.CQRS.Command;

public class RegisterCommand : IRequest<Result<RegisterDto>>
{
    public string Name { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
}