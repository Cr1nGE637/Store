using CSharpFunctionalExtensions;
using MediatR;
using Users.Application.DTOs;

namespace Users.Application.CQRS.Query;

public class GetUserQuery : IRequest<Result<List<GetUserDto>>>
{
    
}