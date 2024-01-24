
namespace Domain.Aggregates.ShowtimeAggregate
{
    public interface IBooking
    {
        DateTime BookingTimeUtc { get; }
        Guid Id { get; }
        ISeatReservation SeatReservation { get; }
        IShowtime Showtime { get; }
    }
}