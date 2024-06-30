namespace APIDtos;

public record SeatWithIdDto
{
    public string SeatNumber { get; }

    public Guid Id { get; }

    public SeatWithIdDto(Guid id, string seatNumber)
    {
        Id = id;
        SeatNumber = seatNumber;
    }
}
