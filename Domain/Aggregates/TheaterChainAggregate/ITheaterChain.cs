using Domain.Aggregates.TheaterAggregate;

namespace Domain.Aggregates.TheaterChainAggregate;

public interface ITheaterChain : IAggregrateRoot
{
    string IdString { get; }

    string Description { get; }
    string Name { get; }

    IMovie AddMovie(string title, string description, string genre, int durationMins, DateTime releaseDateUtc);
    ITheater AddTheater(string name, string location);
    IMovie GetMovieById(int id);
    List<IMovie> GetMovies();
    ITheater? GetTheaterById(int id);
    List<ITheater> GetTheaters();
    void MarkMovieAsAvailable(int id);
    void MarkMovieAsNoLongerAvailable(int id);
    void RemoveTheater(int theaterId);
    void UpdateMovie(int id, string title, string description, string genre, int durationMins, DateTime releaseDateUtc);
    void UpdateTheater(int id, string updatedName, string updatedLocation);
}