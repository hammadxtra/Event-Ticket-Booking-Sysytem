using EventTicketBooking.Application.Interfaces;
using EventTicketBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventTicketBooking.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Entity Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasConversion<string>();
        });

        // Event Entity Configuration
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.RowVersion).IsConcurrencyToken();
        });

        // Booking Entity Configuration
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasOne(b => b.User)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Event)
                  .WithMany(e => e.Bookings)
                  .HasForeignKey(b => b.EventId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Ticket Entity Configuration
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.TicketCode).IsUnique();
            entity.Property(t => t.TicketCode).IsRequired().HasMaxLength(100);

            entity.HasOne(t => t.Booking)
                  .WithMany(b => b.Tickets)
                  .HasForeignKey(t => t.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Event)
                  .WithMany(e => e.Tickets)
                  .HasForeignKey(t => t.EventId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
