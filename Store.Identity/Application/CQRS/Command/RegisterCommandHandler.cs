using CSharpFunctionalExtensions;
using MediatR;
using Store.SharedKernel.Interfaces;
using Users.Application.DTOs;
using Users.Application.Interfaces;
using Users.Domain.Entities;
using Users.Domain.Interfaces;
using Users.Domain.ValueObjects;

namespace Users.Application.CQRS.Command;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterDto>>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(IPasswordHasher passwordHasher, IUsersRepository usersRepository, IUnitOfWork unitOfWork)
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

        return Result.Success(new RegisterDto(
            UserId: userResult.Value.Id,
            Email: userResult.Value.Email,
            Name: userResult.Value.Name,
            Role: userResult.Value.Role.ToString()));
    }
}