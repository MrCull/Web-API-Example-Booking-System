
namespace Domain.Aggregates.TheaterAggregate
{
    public interface ISeat
    {
        Guid Id { get; }
        string SeatNumber { get; }
    }
}