using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.SharedKernel.Interfaces;
using Users.Application.DTOs;
using Users.Application.Interfaces;

namespace Users.Application.CQRS.Command;

public class LoginCommandHandler :  IRequestHandler<LoginCommand, Result<LoginDto>>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginCommandHandler(IUsersRepository usersRepository, IMapper mapper, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<LoginDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByEmailAsync(request.Email);
        var result = _passwordHasher.Verify(request.Password, user.Value.Password);

        if (!result)
        {
            return Result.Failure<LoginDto>("Invalid login attempt");
        }
        
        var token = _jwtProvider.GenerateJwtToken(user.Value);
        
        var loginDto = _mapper.Map<LoginDto>(user);
        loginDto = loginDto with { Token = token };
        return Result.Success(loginDto);
    }
}