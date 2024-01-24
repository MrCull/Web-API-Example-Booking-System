using System.ComponentModel.DataAnnotations;

namespace Domain.Aggregates.ShowtimeAggregate;

internal record Booking(Guid id, DateTime bookingTimeUtc, IShowtime showtime, ISeatReservation seatReservation) : IBooking
{
    [Required]
    public Guid Id { get; private set; } = id;

    public DateTime BookingTimeUtc { get; private set; } = bookingTimeUtc;

    // Navigation properties
    public IShowtime Showtime { get; private set; } = showtime;

    public ISeatReservation SeatReservation { get; private set; } = seatReservation;
}


