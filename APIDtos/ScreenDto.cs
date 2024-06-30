namespace APIDtos;

public record ScreenDto(string ScreenNumber, bool IsEnable, List<SeatDto>? Seats = null)
{
    public string ScreenNumber { get; set; } = ScreenNumber;
    public bool IsEnabled { get; set; } = IsEnable;

    public List<SeatDto> Seats { get; set; } = Seats ?? [];
}
