using CSharpFunctionalExtensions;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Interfaces;
using Identity.Domain.ValueObjects;
using MediatR;

namespace Identity.Application.CQRS.Command;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterDto>>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public RegisterCommandHandler(IPasswordHasher passwordHasher, IUsersRepository usersRepository, IIdentityUnitOfWork unitOfWork)
    {
        _passwordHasher = passwordHasher;
        _usersRepository = usersRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegisterDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var exists = await _usersRepository.ExistsByEmailAsync(request.Email);
        if (exists)
            return Result.Failure<RegisterDto>("Email already exists");

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<RegisterDto>(emailResult.Error);

        var hashedPassword = _passwordHasher.Generate(request.Password);

        var userResult = User.Create(request.Name, emailResult.Value, hashedPassword);
        if (userResult.IsFailure)
            return Result.Failure<RegisterDto>(userResult.Error);

        await _usersRepository.AddAsync(userResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(userResult.Value));
    }

    private static RegisterDto MapToDto(User user) =>
        new(user.Id, user.Name, user.Email, user.Role.ToString());
}