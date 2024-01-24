namespace Domain.Aggregates.TheaterAggregate
{
    public interface ITheater : IAggregrateRoot
    {
        int Id { get; }
        string Location { get; }
        string Name { get; }

        void AddScreen(string screenNumber);
        void AddShowtime(DateTime dateTime, decimal price, Guid screenId, int movieId);
        void DisableScreen(Guid screenId);
        void ReenableScreen(Guid id);
        void RemoveShowtime(int id);
        void UpdateInformation(string newName, string newLocation);
        void UpdateScreen(Guid id, string name);
        void UpdateShowtime(int id, DateTime newDateTime, decimal newPrice, Guid screenId);
    }
}