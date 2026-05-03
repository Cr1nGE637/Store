namespace Store.Identity.Application.DTOs;

public record LoginDto(string Email, string Token, string Role);