using System.ComponentModel.DataAnnotations;

namespace Domain.Aggregates.ShowtimeAggregate;

internal record Booking : IBooking
{
    public Booking(Guid id, DateTime bookingTimeUtc, SeatReservation seatReservation)
    {
        Id = id;
        BookingTimeUtc = bookingTimeUtc;
        SeatReservation = seatReservation;
    }

    [Required]
    public Guid Id { get; private set; }

    public DateTime BookingTimeUtc { get; private set; }

    public ISeatReservation SeatReservation { get; private set; }
}


