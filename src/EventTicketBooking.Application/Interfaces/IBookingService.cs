using EventTicketBooking.Application.DTOs;

namespace EventTicketBooking.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResponseDto> CreateBookingAsync(int userId, CreateBookingRequestDto request);
    Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(int userId);
}
