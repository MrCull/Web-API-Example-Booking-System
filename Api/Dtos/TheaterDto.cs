namespace Api.Dtos;

public class TheaterDto(string name, string location)
{
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
}