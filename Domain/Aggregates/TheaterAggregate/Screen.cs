using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Domain.Aggregates.TheaterAggregate;

public class Screen(int theaterId, string screenNumber) : IScreen
{
    [JsonProperty("id")]
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Required]
    public int TheaterId { get; private set; } = theaterId;

    [Required]
    [StringLength(50, ErrorMessage = "Screen number length can't be more than 50 characters.")]
    public string ScreenNumber { get; private set; } = screenNumber;

    public bool IsEnabled { get; private set; } = true;

    [JsonProperty("seats")]
    internal List<Seat> Seats { get; private set; } = [];

    public void AddSeats(List<string> seatsToAdd)
    {
        Seats.Clear();
        Seats.AddRange(seatsToAdd.Select(s => new Seat(s)));
    }

    public List<ISeat> GetSeats()
        => Seats.Select(s => (ISeat)s).ToList();

    internal void Disable()
    {
        IsEnabled = false;
    }

    internal List<Seat> GetSeatsByNames(List<string> seatNames)
        => Seats.Where(s => seatNames.Contains(s.SeatNumber))
            .ToList();

    internal void Reenable()
    {
        IsEnabled = true;
    }

    internal void UpdateScreenNumber(string screenNumber)
    {
        ScreenNumber = screenNumber;
    }
}
