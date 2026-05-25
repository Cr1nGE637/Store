using Store.Identity.Domain.Aggregates;

namespace Store.Identity.Application.Interfaces;

public interface IJwtProvider
{
    public string GenerateJwtToken(User user);
}