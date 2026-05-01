using CSharpFunctionalExtensions;
using Identity.Application.DTOs;
using MediatR;

namespace Identity.Application.CQRS.Command;

public class RegisterCommand : IRequest<Result<RegisterDto>>
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}