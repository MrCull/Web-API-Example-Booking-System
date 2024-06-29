namespace Domain.Aggregates.ShowtimeAggregate
{
    public interface IShowtime : IAggregrateRoot
    {
        decimal Price { get; }
        DateTime ShowDateTimeUtc { get; }

        int AvailableSeats();
        IBooking CompleteBookingForSeatReservationAndReturnBooking(Guid reservationId);
        int MovieId { get; }
        Guid ScreenId { get; }
        ISeatReservation ProvisionallyReserveSeatsAndReturnReservation(List<string> seatNames);
        List<ISeatReservation> GetSeatReservations();
    }
}