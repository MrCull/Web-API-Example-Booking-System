namespace Domain.Aggregates.TheaterChainAggregate
{
    public interface ITheaterChain
    {
        string Description { get; }
        int Id { get; }
        string Name { get; }

        IMovie AddMovie(string title, string description, string genre, int durationMins, DateTime releaseDateUtc);
        void AddTheater(string name, string location);
        IMovie GetMovieById(int id);
        List<IMovie> GetMovies();
        void MarkMovieAsAvailable(int id);
        void MarkMovieAsNoLongerAvailable(int id);
        void RemoveTheater(int theaterId);
        void UpdateMovie(int id, string title, string description, string genre, int durationMins, DateTime releaseDateUtc);
        void UpdateTheater(int id, string updatedName, string updatedLocation);
    }
}