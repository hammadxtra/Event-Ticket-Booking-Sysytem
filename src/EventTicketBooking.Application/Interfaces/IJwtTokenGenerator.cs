using EventTicketBooking.Domain.Entities;

namespace EventTicketBooking.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, int ExpiresIn) GenerateToken(User user);
}
