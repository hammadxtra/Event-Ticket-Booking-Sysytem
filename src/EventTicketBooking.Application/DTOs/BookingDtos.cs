namespace EventTicketBooking.Application.DTOs;

public record CreateBookingRequestDto(int EventId, int Quantity);

public record TicketResponseDto(int TicketId, string TicketCode);

public record BookingResponseDto(
    int BookingId,
    int EventId,
    string EventTitle,
    int Quantity,
    DateTime BookingDate,
    List<TicketResponseDto> Tickets
);
