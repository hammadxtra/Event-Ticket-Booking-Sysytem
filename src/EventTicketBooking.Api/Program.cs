using System.Text;
using EventTicketBooking.Api.Middleware;
using EventTicketBooking.Application.Interfaces;
using EventTicketBooking.Application.Services;
using EventTicketBooking.Domain.Entities;
using EventTicketBooking.Infrastructure.Data;
using EventTicketBooking.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Infrastructure & Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=event_booking.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// 2. Add Security & JWT Authentication
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

var secretKey = builder.Configuration["JwtSettings:Secret"] 
    ?? "SuperSecretKeyForEventTicketBookingSystem2026ProductionGradeKey!";
var issuer = builder.Configuration["JwtSettings:Issuer"] ?? "EventTicketBookingApi";
var audience = builder.Configuration["JwtSettings:Audience"] ?? "EventTicketBookingClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 3. Register Application Services (Dependency Injection)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 4. Configure Swagger/OpenAPI with JWT Bearer Security
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Event Ticket Booking System API",
        Version = "v1",
        Description = "ASP.NET Core Web API for Event Ticket Reservations with Concurrency Safety"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 5. Build Application Pipeline
var app = builder.Build();

// Ensure Database is Created & Seed Initial Data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();

    if (!dbContext.Events.Any())
    {
        dbContext.Events.AddRange(
            new Event
            {
                Title = "UET Tech Summit 2026",
                Description = "Annual software engineering, AI, and cloud architecture convention.",
                EventDate = DateTime.UtcNow.AddDays(14),
                TotalTickets = 200,
                AvailableTickets = 185
            },
            new Event
            {
                Title = "Aether Music Festival",
                Description = "Live electronic synthwave & futuristic audiovisual experience.",
                EventDate = DateTime.UtcNow.AddDays(30),
                TotalTickets = 500,
                AvailableTickets = 500
            },
            new Event
            {
                Title = "AI & Autonomous Systems Expo",
                Description = "Exhibition of advanced generative AI models and robotics.",
                EventDate = DateTime.UtcNow.AddDays(45),
                TotalTickets = 150,
                AvailableTickets = 12
            }
        );
        dbContext.SaveChanges();
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Ticket Booking API v1");
    });
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
