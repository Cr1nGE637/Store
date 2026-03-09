using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.SharedKernel.Interfaces;
using Users.Application.DTOs;
using Users.Application.Interfaces;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.CQRS.Command;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterDto>>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHasher _passwordHasher;
    
    public RegisterCommandHandler(IPasswordHasher passwordHasher, IUsersRepository usersRepository)
    {
        _passwordHasher = passwordHasher;
        _usersRepository = usersRepository;
    }

    public async Task<Result<RegisterDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var exists = await _usersRepository.ExistsByEmailAsync(request.Email);
        if (exists)
        {
            return Result.Failure<RegisterDto>("Email already exists");
        }
        var userEmail = Email.Create(request.Email);
        var hashedPassword = _passwordHasher.Generate(request.Password);
        var user = User.Create(request.Name, userEmail.Value, hashedPassword);
        
        await _usersRepository.AddAsync(user.Value);
        
        var registerDto = new RegisterDto(
            UserId: user.Value.Id,
            Email: user.Value.Email,
            Name:  user.Value.Name);
        
        return Result.Success(registerDto);
    }
}