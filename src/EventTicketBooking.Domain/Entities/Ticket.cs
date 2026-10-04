namespace EventTicketBooking.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking? Booking { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public string TicketCode { get; set; } = string.Empty;
}
