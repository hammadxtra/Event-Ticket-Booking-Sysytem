using EventTicketBooking.Application.DTOs;
using EventTicketBooking.Application.Services;
using EventTicketBooking.Domain.Entities;
using EventTicketBooking.Domain.Exceptions;
using EventTicketBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventTicketBooking.Tests;

public class BookingServiceTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowException_WhenQuantityInvalid()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var bookingService = new BookingService(context);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => 
            bookingService.CreateBookingAsync(1, new CreateBookingRequestDto(1, 0)));

        await Assert.ThrowsAsync<DomainException>(() => 
            bookingService.CreateBookingAsync(1, new CreateBookingRequestDto(1, 15)));
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowException_WhenEventNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var bookingService = new BookingService(context);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => 
            bookingService.CreateBookingAsync(1, new CreateBookingRequestDto(999, 2)));
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldCreateBookingAndTickets_WhenValid()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var ev = new Event
        {
            Id = 1,
            Title = "Tech Summit 2026",
            EventDate = DateTime.UtcNow.AddDays(10),
            TotalTickets = 100,
            AvailableTickets = 100
        };
        context.Events.Add(ev);
        await context.SaveChangesAsync();

        var bookingService = new BookingService(context);

        // Act
        var result = await bookingService.CreateBookingAsync(1, new CreateBookingRequestDto(1, 2));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.EventId);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(2, result.Tickets.Count);
        Assert.StartsWith("TKT-1-", result.Tickets[0].TicketCode);
    }
}
