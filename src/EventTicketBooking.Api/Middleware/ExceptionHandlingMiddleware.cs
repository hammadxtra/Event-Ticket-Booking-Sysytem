using System.Net;
using System.Text.Json;
using EventTicketBooking.Domain.Exceptions;

namespace EventTicketBooking.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during HTTP request processing.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, type) = exception switch
        {
            InsufficientTicketsException => (
                HttpStatusCode.Conflict,
                "Insufficient Ticket Capacity",
                "https://tools.ietf.org/html/rfc7231#section-6.5.8"
            ),
            DomainException => (
                HttpStatusCode.BadRequest,
                "Bad Request - Domain Constraint Violation",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Unauthorized Access",
                "https://tools.ietf.org/html/rfc7235#section-3.1"
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new
        {
            type,
            title,
            status = (int)statusCode,
            detail = exception.Message,
            instance = context.Request.Path.Value,
            traceId = context.TraceIdentifier
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
    }
}
