namespace APIDtos;

public class BookingDto
{
    public BookingDto(Guid id, DateTime bookingTimeUtc, ShowtimeWithIdDto showtime, SeatReservationWithIdDto seatReservation)
    {
        Id = id;
        BookingTimeUtc = bookingTimeUtc;
        Showtime = showtime;
        SeatReservation = seatReservation;
    }

    public Guid Id { get; set; }
    public DateTime BookingTimeUtc { get; set; }
    public ShowtimeWithIdDto Showtime { get; set; }
    public SeatReservationWithIdDto SeatReservation { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}, BookingTimeUtc: {BookingTimeUtc}, Showtime: {Showtime}, SeatReservation: {SeatReservation}";
    }
}
