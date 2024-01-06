namespace Api.Dtos;

public record MovieDto
{
    public MovieDto(int id, string title, string description, TimeSpan duration, string genre, DateTime releaseDateUtc, MovieStatus movieStatus)
    {
        Id = id;
        Title = title;
        Description = description;
        Duration = duration;
        Genre = genre;
        ReleaseDateUtc = releaseDateUtc;
        MovieStatus = movieStatus;
    }

    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeSpan Duration { get; set; }
    public string Genre { get; set; }
    public DateTime ReleaseDateUtc { get; set; }
    public MovieStatus MovieStatus { get; set; }
}

public enum MovieStatus
{
    Available,
    NoLongerAvailable
}