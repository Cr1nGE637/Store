using Identity.Domain.Entities;

namespace Identity.Application.Interfaces;

public interface IJwtProvider
{
    public string GenerateJwtToken(User user);
}