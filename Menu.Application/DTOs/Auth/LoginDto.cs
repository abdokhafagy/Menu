namespace Menu.Application.DTOs.Auth;

public record LoginDto(string Email, string Password, string? Device);
