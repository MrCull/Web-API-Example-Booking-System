namespace Api.Dtos;

public record SeatReservationDto
{
    public List<string> Seats;
    public Guid ShowtimeId;

    public SeatReservationDto(List<string> seats, Guid showtimeId)
    {
        Seats = seats;
        ShowtimeId = showtimeId;
    }

}
