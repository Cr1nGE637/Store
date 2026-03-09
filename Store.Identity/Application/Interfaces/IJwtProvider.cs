using Users.Domain.Entities;

namespace Users.Application.Interfaces;

public interface IJwtProvider
{
    public string GenerateJwtToken(User user);
}