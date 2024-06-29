using System.ComponentModel.DataAnnotations;

namespace Domain.Aggregates.ShowtimeAggregate;

internal record Booking(Guid Id, DateTime BookingTimeUtc, IShowtime Showtime, ISeatReservation SeatReservation) : IBooking
{
    [Required]
    public Guid Id { get; private set; } = Id;

    public DateTime BookingTimeUtc { get; private set; } = BookingTimeUtc;

    // Navigation properties
    public IShowtime Showtime { get; private set; } = Showtime;

    public ISeatReservation SeatReservation { get; private set; } = SeatReservation;
}


