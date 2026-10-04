namespace EventTicketBooking.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int Quantity { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
