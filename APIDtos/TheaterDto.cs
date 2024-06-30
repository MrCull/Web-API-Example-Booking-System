namespace APIDtos;

public class TheaterDto(string name, string location, List<ScreenWithIdDto>? screensWithIdDto = null)
{
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;

    public List<ScreenWithIdDto> ScreensWithIdDto { get; set; } = screensWithIdDto ?? [];
}
