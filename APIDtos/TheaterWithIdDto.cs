namespace APIDtos;
public class TheaterWithIdDto(int id, string name, string location, List<ScreenWithIdDto> screensWithIdDto)
    : TheaterDto(name, location, screensWithIdDto)
{
    public int Id { get; set; } = id;
}