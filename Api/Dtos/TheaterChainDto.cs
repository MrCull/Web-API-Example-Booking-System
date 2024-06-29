namespace Api.Dtos;

public record TheaterChainDto
{
    public TheaterChainDto(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public int Id { get; }
    public string Name { get; set; }
    public string Description { get; set; }
}
