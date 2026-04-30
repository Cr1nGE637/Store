using CSharpFunctionalExtensions;
using MediatR;
using Users.Application.DTOs;
using Users.Application.Interfaces;
using Users.Domain.Interfaces;

namespace Users.Application.CQRS.Command;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginDto>>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginCommandHandler(IUsersRepository usersRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<LoginDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _usersRepository.GetByEmailAsync(request.Email);
        if (userResult.IsFailure)
            return Result.Failure<LoginDto>("Invalid email or password");

        var passwordValid = _passwordHasher.Verify(request.Password, userResult.Value.Password);
        if (!passwordValid)
            return Result.Failure<LoginDto>("Invalid email or password");

        var token = _jwtProvider.GenerateJwtToken(userResult.Value);
        return Result.Success(new LoginDto(userResult.Value.Email, token, userResult.Value.Role.ToString()));
    }
}