using CSharpFunctionalExtensions;
using MediatR;
using Users.Application.DTOs;
using Users.Domain.Interfaces;

namespace Users.Application.CQRS.Query;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<GetUserDto>>
{
    private readonly IUsersRepository _usersRepository;

    public GetUserQueryHandler(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Result<GetUserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var userResult = await _usersRepository.GetByEmailAsync(request.Email);
        if (userResult.IsFailure)
            return Result.Failure<GetUserDto>(userResult.Error);

        return Result.Success(new GetUserDto(userResult.Value.Email, userResult.Value.Name, userResult.Value.Role.ToString()));
    }
}