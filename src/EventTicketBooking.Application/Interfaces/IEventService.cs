using EventTicketBooking.Application.DTOs;

namespace EventTicketBooking.Application.Interfaces;

public interface IEventService
{
    Task<EventResponseDto> CreateEventAsync(CreateEventDto request);
    Task<IEnumerable<EventResponseDto>> GetAllEventsAsync();
    Task<EventResponseDto?> GetEventByIdAsync(int id);
}
