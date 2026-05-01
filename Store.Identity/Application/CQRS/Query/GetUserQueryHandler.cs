using CSharpFunctionalExtensions;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Interfaces;
using MediatR;

namespace Identity.Application.CQRS.Query;

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

        return Result.Success(MapToDto(userResult.Value));
    }

    private static GetUserDto MapToDto(User user) => new(user.Email, user.Name, user.Role.ToString());
}