namespace Api.Dtos;
public class TheaterWithIdDto(int id, string name, string location)
    : TheaterDto(name, location)
{
    public int Id { get; set; } = id;
}