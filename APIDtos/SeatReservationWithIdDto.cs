namespace APIDtos;

public record SeatReservationWithIdDto
{
    public Guid Id { get; set; }
    public List<string> Seats { get; set; }
    public int ShowtimeId { get; set; }
    public DateTime? ReservationTimeoutUtc { get; set; }
    public SeatReservationStatus Status { get; set; }

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
