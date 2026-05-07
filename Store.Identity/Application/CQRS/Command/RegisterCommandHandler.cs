using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Store.Identity.Application.DTOs;
using Store.Identity.Application.Interfaces;
using Store.Identity.Domain.Aggregates;
using Store.Identity.Domain.Interfaces;
using Store.Identity.Domain.ValueObjects;

namespace Store.Identity.Application.CQRS.Command;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterDto>>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IIdentityDomainEventOutbox _outbox;

    public RegisterCommandHandler(
        IPasswordHasher passwordHasher,
        IUsersRepository usersRepository,
        IIdentityUnitOfWork unitOfWork,
        IIdentityDomainEventOutbox outbox)
    {
        _passwordHasher = passwordHasher;
        _usersRepository = usersRepository;
        _unitOfWork = unitOfWork;
        _outbox = outbox;
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

        await _outbox.AddAsync(userResult.Value.DomainEvents, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            userResult.Value.ClearDomainEvents();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Result.Failure<RegisterDto>("Email already exists");
        }

        return Result.Success(IdentityMappings.ToRegisterDto(userResult.Value));
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}
