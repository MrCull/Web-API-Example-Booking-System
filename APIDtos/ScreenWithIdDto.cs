namespace APIDtos;

public record ScreenWithIdDto(Guid Id, string ScreenNumber, bool IsEnable, List<SeatWithIdDto> Seats)
{
    public Guid Id { get; set; } = Id;
    public string ScreenNumber { get; set; } = ScreenNumber;
    public bool IsEnabled { get; set; } = IsEnable;

    public List<SeatWithIdDto> Seats { get; set; } = Seats ?? [];
}
