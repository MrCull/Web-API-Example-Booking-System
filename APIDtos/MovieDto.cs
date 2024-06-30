namespace APIDtos;

public record MovieDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int DurationMins { get; set; }
    public string Genre { get; set; }
    public DateTime ReleaseDateUtc { get; set; }
    public MovieStatus MovieStatus { get; set; }

    public MovieDto(string title, string description, int durationMins, string genre, DateTime releaseDateUtc, MovieStatus movieStatus = MovieStatus.Available)
    {
        Title = title;
        Description = description;
        DurationMins = durationMins;
        Genre = genre;
        ReleaseDateUtc = releaseDateUtc;
        MovieStatus = movieStatus;
    }
}

public enum MovieStatus
{
    Available,
    NoLongerAvailable
}