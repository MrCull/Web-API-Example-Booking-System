
namespace Domain.Aggregates.TheaterChainAggregate;

public interface IMovie
{
    string Description { get; }
    int DurationMins { get; }
    string Genre { get; }
    int Id { get; }
    DateTime ReleaseDateUtc { get; }
    TheaterChainMovieStatus TheaterChainMovieStatus { get; }
    string Title { get; }

    internal void MarkAsAvailable();
    internal void MarkAsNoLongerAvailable();
}