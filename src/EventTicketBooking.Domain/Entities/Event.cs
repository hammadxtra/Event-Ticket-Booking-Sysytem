namespace EventTicketBooking.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int TotalTickets { get; set; }
    public int AvailableTickets { get; set; }
    public byte[] RowVersion { get; set; } = Guid.NewGuid().ToByteArray();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public bool HasAvailableCapacity(int requestedQuantity)
    {
        return AvailableTickets >= requestedQuantity;
    }
}
