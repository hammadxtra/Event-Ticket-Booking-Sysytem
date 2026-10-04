using System.Security.Claims;
using EventTicketBooking.Application.DTOs;
using EventTicketBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketBooking.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid authentication claims." });
        }

        var booking = await _bookingService.CreateBookingAsync(userId, request);
        return CreatedAtAction(nameof(GetUserBookings), new { id = booking.BookingId }, booking);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetUserBookings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid authentication claims." });
        }

        var bookings = await _bookingService.GetUserBookingsAsync(userId);
        return Ok(bookings);
    }
}
