using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventTicketBooking.Application.Interfaces;
using EventTicketBooking.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EventTicketBooking.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, int ExpiresIn) GenerateToken(User user)
    {
        var secretKey = _configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForEventTicketBookingSystem2026ProductionGradeKey!";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "EventTicketBookingApi";
        var audience = _configuration["JwtSettings:Audience"] ?? "EventTicketBookingClient";
        var expiresInMinutes = int.TryParse(_configuration["JwtSettings:ExpiresInMinutes"], out var min) ? min : 1440;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiresInMinutes * 60);
    }
}
