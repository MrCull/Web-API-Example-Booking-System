namespace APIDtos;

public class BookingDto
{
    public BookingDto(Guid id, DateTime bookingTimeUtc, SeatReservationWithIdDto seatReservation)
    {
        Id = id;
        BookingTimeUtc = bookingTimeUtc;
        SeatReservation = seatReservation;
    }

    public Guid Id { get; set; }
    public DateTime BookingTimeUtc { get; set; }
    public SeatReservationWithIdDto SeatReservation { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}, BookingTimeUtc: {BookingTimeUtc}, SeatReservation: {SeatReservation}";
    }
}
