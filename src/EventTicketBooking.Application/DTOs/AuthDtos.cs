namespace EventTicketBooking.Application.DTOs;

public record RegisterRequestDto(string Email, string Password);

public record LoginRequestDto(string Email, string Password);

public record AuthTokenResponseDto(string Token, string TokenType, int ExpiresIn, string Email, string Role);

public record UserResponseDto(int Id, string Email, string Role, DateTime CreatedAt);
