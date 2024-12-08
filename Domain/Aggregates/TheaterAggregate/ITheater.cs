
using Domain.Aggregates.ShowtimeAggregate;
using Domain.Aggregates.TheaterChainAggregate;

namespace Domain.Aggregates.TheaterAggregate;

public interface ITheater : IAggregrateRoot
{
    string Location { get; }
    string Name { get; }
    IScreen AddScreen(string screenNumber);
    IScreen AddScreen(string screenNumber, List<string> seats);
    IShowtime AddShowtime(DateTime dateTime, decimal price, Guid screenId, int movieId);
    void DisableScreen(Guid screenId);
    IShowtime? GetActiveShowtimeById(int showtimeId);
    IEnumerable<IShowtime> GetActiveShowtimes();
    IScreen? GetScreenById(Guid screenId);
    List<IScreen> GetScreens();
    void ReenableScreen(Guid id);
    void RemoveShowtime(int id);
    void Initialize(List<Movie> movies);
    void UpdateInformation(string newName, string newLocation);
    IScreen UpdateScreen(Guid id, string name, List<string> seats);
    void UpdateShowtime(int id, DateTime newDateTime, decimal newPrice, Guid screenId);
}