namespace Domain.Aggregates.ShowtimeAggregate
{
    public interface IShowtime : IAggregrateRoot
    {
        decimal Price { get; }
        DateTime ShowDateTimeUtc { get; }

        int AvailableSeats();
        IBooking CompleteBookingForSeatReservationAndReturnBooking(Guid reservationId);
        internal ISeatReservation ProvisionallyReserveSeatsAndReturnReservation(List<string> seatNames);

    }
}