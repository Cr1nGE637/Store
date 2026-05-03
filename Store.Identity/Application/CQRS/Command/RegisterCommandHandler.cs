using CSharpFunctionalExtensions;
using Store.Identity.Application.DTOs;
using Store.Identity.Application.Interfaces;
using Store.Identity.Domain.Aggregates;
using Store.Identity.Domain.Interfaces;
using Store.Identity.Domain.ValueObjects;
using MediatR;

namespace Store.Identity.Application.CQRS.Command;

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
        if (request.Password.Length < 8)
            return Result.Failure<RegisterDto>("Password must be at least 8 characters");

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<RegisterDto>(emailResult.Error);

        var exists = await _usersRepository.ExistsByEmailAsync(emailResult.Value.Value);
        if (exists)
            return Result.Failure<RegisterDto>("Email already exists");

        var hashedPassword = _passwordHasher.Generate(request.Password);

        var userResult = User.Create(request.Name, emailResult.Value, hashedPassword);
        if (userResult.IsFailure)
            return Result.Failure<RegisterDto>(userResult.Error);

        var addResult = await _usersRepository.AddAsync(userResult.Value);
        if (addResult.IsFailure)
            return Result.Failure<RegisterDto>(addResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(IdentityMappings.ToRegisterDto(userResult.Value));
    }
}