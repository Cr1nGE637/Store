using CSharpFunctionalExtensions;
using MediatR;
using Users.Application.DTOs;

namespace Users.Application.CQRS.Command;

public class RegisterCommand : IRequest<Result<RegisterDto>>
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}