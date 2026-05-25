using CSharpFunctionalExtensions;
using Store.Identity.Application.DTOs;
using Store.Identity.Domain.Aggregates;
using Store.Identity.Domain.Interfaces;
using Store.Identity.Domain.ValueObjects;
using MediatR;

namespace Store.Identity.Application.CQRS.Query;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<GetUserDto>>
{
    private readonly IUsersRepository _usersRepository;

    public GetUserQueryHandler(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Result<GetUserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<GetUserDto>(emailResult.Error);

        var userResult = await _usersRepository.GetByEmailAsync(emailResult.Value.Value);
        if (userResult.IsFailure)
            return Result.Failure<GetUserDto>(userResult.Error);

        return Result.Success(IdentityMappings.ToGetUserDto(userResult.Value));
    }
}