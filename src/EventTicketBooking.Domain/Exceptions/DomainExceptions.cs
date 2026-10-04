namespace EventTicketBooking.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InsufficientTicketsException : DomainException
{
    public InsufficientTicketsException(string message) : base(message) { }
}
