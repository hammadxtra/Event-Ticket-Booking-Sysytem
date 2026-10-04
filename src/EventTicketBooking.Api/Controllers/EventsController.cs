using EventTicketBooking.Application.DTOs;
using EventTicketBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketBooking.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _eventService.GetAllEventsAsync();
        return Ok(events);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventById(int id)
    {
        var ev = await _eventService.GetEventByIdAsync(id);
        if (ev == null)
        {
            return NotFound(new { message = $"Event with ID {id} was not found." });
        }
        return Ok(ev);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto request)
    {
        var result = await _eventService.CreateEventAsync(request);
        return CreatedAtAction(nameof(GetEventById), new { id = result.Id }, result);
    }
}
