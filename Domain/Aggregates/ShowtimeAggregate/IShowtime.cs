namespace Domain.Aggregates.ShowtimeAggregate
{
    public interface IShowtime
    {
        int Id { get; }
        decimal Price { get; }
        DateTime ShowDateTimeUtc { get; }

        int AvailableSeats();
        IBooking CompleteBookingForSeatReservationAndReturnBooking(Guid reservationId);
        internal ISeatReservation ProvisionallyReserveSeatsAndReturnReservation(List<string> seatNames);

    }
}