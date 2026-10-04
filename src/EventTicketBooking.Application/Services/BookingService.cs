using EventTicketBooking.Application.DTOs;
using EventTicketBooking.Application.Interfaces;
using EventTicketBooking.Domain.Entities;
using EventTicketBooking.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EventTicketBooking.Application.Services;

public class BookingService : IBookingService
{
    private readonly IAppDbContext _context;

    public BookingService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<BookingResponseDto> CreateBookingAsync(int userId, CreateBookingRequestDto request)
    {
        if (request.Quantity <= 0 || request.Quantity > 10)
        {
            throw new DomainException("Quantity must be between 1 and 10 tickets per booking request.");
        }

        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == request.EventId);
        if (ev == null)
        {
            throw new DomainException($"Event with ID {request.EventId} was not found.");
        }

        if (ev.EventDate <= DateTime.UtcNow)
        {
            throw new DomainException("Cannot book tickets for events that have already taken place.");
        }

        var isRelational = _context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory";
        using var transaction = isRelational ? await _context.Database.BeginTransactionAsync() : null;

        try
        {
            if (isRelational)
            {
                // Atomic UPDATE predicate enforcing AvailableTickets >= requested.Quantity
                int rowsAffected = await _context.Events
                    .Where(e => e.Id == request.EventId && e.AvailableTickets >= request.Quantity)
                    .ExecuteUpdateAsync(s => s.SetProperty(
                        e => e.AvailableTickets,
                        e => e.AvailableTickets - request.Quantity
                    ));

                if (rowsAffected == 0)
                {
                    throw new InsufficientTicketsException($"Insufficient tickets available for event '{ev.Title}'. Requested: {request.Quantity}, Available: {ev.AvailableTickets}.");
                }
            }
            else
            {
                if (ev.AvailableTickets < request.Quantity)
                {
                    throw new InsufficientTicketsException($"Insufficient tickets available for event '{ev.Title}'. Requested: {request.Quantity}, Available: {ev.AvailableTickets}.");
                }
                ev.AvailableTickets -= request.Quantity;
            }

            var booking = new Booking
            {
                UserId = userId,
                EventId = request.EventId,
                Quantity = request.Quantity,
                BookingDate = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var tickets = new List<Ticket>();
            for (int i = 0; i < request.Quantity; i++)
            {
                var code = $"TKT-{request.EventId}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                var ticket = new Ticket
                {
                    BookingId = booking.Id,
                    EventId = request.EventId,
                    TicketCode = code
                };
                tickets.Add(ticket);
                _context.Tickets.Add(ticket);
            }

            await _context.SaveChangesAsync();

            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return new BookingResponseDto(
                booking.Id,
                ev.Id,
                ev.Title,
                booking.Quantity,
                booking.BookingDate,
                tickets.Select(t => new TicketResponseDto(t.Id, t.TicketCode)).ToList()
            );
        }
        catch
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
            throw;
        }
    }

    public async Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(int userId)
    {
        var bookings = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Event)
            .Include(b => b.Tickets)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return bookings.Select(b => new BookingResponseDto(
            b.Id,
            b.EventId,
            b.Event?.Title ?? "Event",
            b.Quantity,
            b.BookingDate,
            b.Tickets.Select(t => new TicketResponseDto(t.Id, t.TicketCode)).ToList()
        ));
    }
}
