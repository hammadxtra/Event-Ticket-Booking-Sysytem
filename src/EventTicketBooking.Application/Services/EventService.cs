using EventTicketBooking.Application.DTOs;
using EventTicketBooking.Application.Interfaces;
using EventTicketBooking.Domain.Entities;
using EventTicketBooking.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EventTicketBooking.Application.Services;

public class EventService : IEventService
{
    private readonly IAppDbContext _context;

    public EventService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<EventResponseDto> CreateEventAsync(CreateEventDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new DomainException("Event title is required.");
        }

        if (request.TotalTickets <= 0)
        {
            throw new DomainException("Total tickets must be greater than zero.");
        }

        if (request.EventDate <= DateTime.UtcNow)
        {
            throw new DomainException("Event date must be in the future.");
        }

        var newEvent = new Event
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            EventDate = request.EventDate,
            TotalTickets = request.TotalTickets,
            AvailableTickets = request.TotalTickets
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return MapToDto(newEvent);
    }

    public async Task<IEnumerable<EventResponseDto>> GetAllEventsAsync()
    {
        var events = await _context.Events
            .AsNoTracking()
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        return events.Select(MapToDto);
    }

    public async Task<EventResponseDto?> GetEventByIdAsync(int id)
    {
        var ev = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        return ev == null ? null : MapToDto(ev);
    }

    private static EventResponseDto MapToDto(Event e)
    {
        return new EventResponseDto(e.Id, e.Title, e.Description, e.EventDate, e.TotalTickets, e.AvailableTickets);
    }
}
