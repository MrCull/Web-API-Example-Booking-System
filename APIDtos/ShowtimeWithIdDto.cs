namespace APIDtos;
public record ShowtimeWithIdDto : ShowtimeDto
{
    public ShowtimeWithIdDto(int id, int movieId, Guid screenId, DateTime showDateTimeUtc, decimal price)
        : base(movieId, screenId, showDateTimeUtc, price)
    {
        Id = id;
    }

    public int Id { get; set; }


    public override string ToString()
    {
        return $"Id: {Id}, MovieId: {MovieId}, ScreenId: {ScreenId}, ShowDateTimeUtc: {ShowDateTimeUtc}, Price: {Price}";
    }
}