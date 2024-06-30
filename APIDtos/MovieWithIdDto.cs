namespace APIDtos;

public record MovieWithIdDto : MovieDto
{
    public MovieWithIdDto(int id, string title, string description, int durationMins, string genre, DateTime releaseDateUtc, MovieStatus movieStatus)
        : base(title, description, durationMins, genre, releaseDateUtc, movieStatus)
    {
        Id = id;
    }

    public int Id { get; set; }
}