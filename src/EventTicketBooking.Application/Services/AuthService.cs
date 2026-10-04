using EventTicketBooking.Application.DTOs;
using EventTicketBooking.Application.Interfaces;
using EventTicketBooking.Domain.Entities;
using EventTicketBooking.Domain.Enums;
using EventTicketBooking.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EventTicketBooking.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(IAppDbContext context, IJwtTokenGenerator tokenGenerator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            throw new DomainException("Valid email address is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            throw new DomainException("Password must be at least 6 characters long.");
        }

        var existingUser = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (existingUser)
        {
            throw new DomainException("An account with this email already exists.");
        }

        var isFirstUser = !await _context.Users.AnyAsync();
        var role = isFirstUser ? UserRole.Admin : UserRole.User;

        var user = new User
        {
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserResponseDto(user.Id, user.Email, user.Role.ToString(), user.CreatedAt);
    }

    public async Task<AuthTokenResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new DomainException("Invalid email or password.");
        }

        var (token, expiresIn) = _tokenGenerator.GenerateToken(user);
        return new AuthTokenResponseDto(token, "Bearer", expiresIn, user.Email, user.Role.ToString());
    }
}
