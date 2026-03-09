using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Users.Application.DTOs;
using Users.Application.Interfaces;

namespace Users.Application.CQRS.Query;

public class GetCustomerQueryHandler : IRequestHandler<GetUserQuery, Result<List<GetUserDto>>>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;

    public GetCustomerQueryHandler(IUsersRepository usersRepository, IMapper mapper)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<GetUserDto>>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var users = await _usersRepository.GetByEmailAsync("asdasd"); 
        var userDtos = _mapper.Map<List<GetUserDto>>(users.Value);
        return Result.Success(userDtos);
    }
}