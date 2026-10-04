using EventTicketBooking.Application.DTOs;
using EventTicketBooking.Application.Services;
using EventTicketBooking.Domain.Exceptions;
using EventTicketBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventTicketBooking.Tests;

public class EventServiceTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateEventAsync_ShouldThrowException_WhenTitleEmpty()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        var request = new CreateEventDto("", "Desc", DateTime.UtcNow.AddDays(5), 50);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => service.CreateEventAsync(request));
    }

    [Fact]
    public async Task CreateEventAsync_ShouldThrowException_WhenEventDateInPast()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        var request = new CreateEventDto("Past Event", "Desc", DateTime.UtcNow.AddDays(-1), 50);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => service.CreateEventAsync(request));
    }

    [Fact]
    public async Task CreateEventAsync_ShouldSuccessfullyPersistEvent()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        var request = new CreateEventDto("Future Conf", "Awesome Conf", DateTime.UtcNow.AddDays(5), 200);

        // Act
        var result = await service.CreateEventAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Future Conf", result.Title);
        Assert.Equal(200, result.TotalTickets);
        Assert.Equal(200, result.AvailableTickets);
    }
}
