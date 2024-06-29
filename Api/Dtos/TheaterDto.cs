namespace Api.Dtos;

public class TheaterDto(string name, string location, List<ScreenWithIdDto> screens)
{
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;

    public List<ScreenWithIdDto> Screens { get; set; } = screens;
}
