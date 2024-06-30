namespace APIDtos;

public record SeatReservationWithIdDto
{
    public Guid Id;
    public List<string> Seats;
    public int ShowtimeId;
    public DateTime? ReservationTimeoutUtc;
    public SeatReservationStatus Status;

    public SeatReservationWithIdDto(Guid id, List<string> seats, int showtimeId, DateTime? reservationTimeoutUtc, SeatReservationStatus status)
    {
        Id = id;
        Seats = seats;
        ShowtimeId = showtimeId;
        ReservationTimeoutUtc = reservationTimeoutUtc;
        Status = status;
    }

    public enum SeatReservationStatus
    {
        Provisional,
        Confirmed
    }

    public override string ToString()
    {
        return $"Id: {Id}, Seats: {Seats}, ShowtimeId: {ShowtimeId}, ReservationTimeoutUtc: {ReservationTimeoutUtc}, Status: {Status}";
    }
}
