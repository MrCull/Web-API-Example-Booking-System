
namespace Domain.Aggregates.TheaterAggregate;

public interface IScreen
{
    Guid Id { get; }
    bool IsEnabled { get; }
    string ScreenNumber { get; }
    int TheaterId { get; }

    void AddSeats(List<string> seatsToAdd);
    List<ISeat> GetSeats();
}