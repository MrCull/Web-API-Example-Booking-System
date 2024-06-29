// DTO for the Showtime.cs entity
public record ShowtimeWithIdDto
{
    public ShowtimeWithIdDto(int id, int movieId, Guid screenId, DateTime showDateTimeUtc, decimal price)
    {
        Id = id;
        MovieId = movieId;
        ScreenId = screenId;
        ShowDateTimeUtc = showDateTimeUtc;
        Price = price;
    }

    public int Id { get; set; }
    public int MovieId { get; set; }
    public Guid ScreenId { get; set; }
    public DateTime ShowDateTimeUtc { get; set; }
    public decimal Price { get; set; }

}