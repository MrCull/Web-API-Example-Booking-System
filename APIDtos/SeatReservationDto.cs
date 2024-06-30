namespace APIDtos;

public record SeatReservationDto
{
    public List<string> Seats { get; set; }

    public int ShowtimeId { get; set; }

    public SeatReservationDto(List<string> seats, int showtimeId)
    {
        Seats = seats;
        ShowtimeId = showtimeId;
    }

}
