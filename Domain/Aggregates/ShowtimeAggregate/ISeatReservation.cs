using Domain.Aggregates.TheaterAggregate;

namespace Domain.Aggregates.ShowtimeAggregate;

public interface ISeatReservation
{
    Guid Id { get; }
    decimal Price { get; }
    DateTime? ReservationTimeoutUtc { get; }
    DateTime ReservationTimeUtc { get; }
    int ShowtimeId { get; }
    ReservationStatus Status { get; }

    List<ISeat> GetSeats();
    void SetShowtime(IShowtime showtime);
    internal void SetReservationTimeout(DateTime reservationTimeoutUtc);
}