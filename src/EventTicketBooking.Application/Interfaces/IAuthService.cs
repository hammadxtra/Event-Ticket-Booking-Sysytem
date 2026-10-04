using EventTicketBooking.Application.DTOs;

namespace EventTicketBooking.Application.Interfaces;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthTokenResponseDto> LoginAsync(LoginRequestDto request);
}
