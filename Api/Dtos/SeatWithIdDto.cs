namespace Api.Dtos
{
    public record SeatWithIdDto(Guid Id, string SeatNumber)
    {
        public Guid Id { get; } = Id;
        public string SeatNumber { get; } = SeatNumber;
    }
}
