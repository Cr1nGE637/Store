namespace Store.Identity.Application.DTOs;

public record RegisterDto(Guid UserId, string Name, string Email, string Role);