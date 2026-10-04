namespace EventTicketBooking.Application.DTOs;

public record CreateEventDto(string Title, string Description, DateTime EventDate, int TotalTickets);

public record EventResponseDto(
    int Id,
    string Title,
    string Description,
    DateTime EventDate,
    int TotalTickets,
    int AvailableTickets
);
