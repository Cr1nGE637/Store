using Store.Identity.Application.DTOs;
using Store.Identity.Domain.Aggregates;

namespace Store.Identity.Application;

internal static class IdentityMappings
{
    internal static RegisterDto ToRegisterDto(User user) =>
        new(user.Id, user.Name, user.Email.Value, user.Role.ToString());

    internal static LoginDto ToLoginDto(User user, string token) =>
        new(user.Email.Value, token, user.Role.ToString());

    internal static GetUserDto ToGetUserDto(User user) =>
        new(user.Email.Value, user.Name, user.Role.ToString());
}
