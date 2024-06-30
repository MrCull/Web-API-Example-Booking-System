namespace APIDtos;
public record ShowtimeDto
{
    public ShowtimeDto(int movieId, Guid screenId, DateTime showDateTimeUtc, decimal price)
    {
        MovieId = movieId;
        ScreenId = screenId;
        ShowDateTimeUtc = showDateTimeUtc;
        Price = price;
    }

    public int MovieId { get; set; }
    public Guid ScreenId { get; set; }
    public DateTime ShowDateTimeUtc { get; set; }
    public decimal Price { get; set; }
}