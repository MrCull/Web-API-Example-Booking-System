using System.ComponentModel.DataAnnotations;

namespace Domain.Aggregates.ShowtimeAggregate;

public class Booking(Guid id, int showtimeId, DateTime bookingTimeUtc, Showtime showtime, SeatReservation seatReservation)
{
    [Required]
    public Guid Id { get; private set; } = id;

    public DateTime BookingTimeUtc { get; private set; } = bookingTimeUtc;

    // Navigation properties
    public Showtime Showtime { get; private set; } = showtime;

    public SeatReservation SeatReservation { get; private set; } = seatReservation;
}


