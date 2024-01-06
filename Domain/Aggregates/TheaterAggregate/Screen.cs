using System.ComponentModel.DataAnnotations;
using Domain.Aggregates.ShowtimeAggregate;

namespace Domain.Aggregates.TheaterAggregate;

public class Screen(int theaterId, string screenNumber)
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Required]
    public int TheaterId { get; private set; } = theaterId;

    [Required]
    [StringLength(50, ErrorMessage = "Screen number length can't be more than 50 characters.")]
    public string ScreenNumber { get; private set; } = screenNumber;

    public bool IsEnabled { get; private set; } = true;

    // Navigation property for showtimes
    internal List<Showtime> Showtimes { get; private set; } = [];

    internal List<Seat> Seats { get; private set; } = [];

    public void AddSeats(List<string> seatsToAdd)
    {
        Seats.AddRange(seatsToAdd.Select(s => new Seat(s)));
    }

    internal void AddShowtime(Showtime showtime)
    {
        Showtimes.Add(showtime);
    }

    internal void Disable()
    {
        IsEnabled = false;
    }

    internal List<Seat> GetSeatsByNames(List<string> seatNames)
        => Seats.Where(s => seatNames.Contains(s.SeatNumber)).ToList();

    internal bool HasFutureShowtimes()
        => Showtimes?.Any(s => s.ShowDateTimeUtc > DateTime.UtcNow) ?? false;

    internal void Reenable()
    {
        IsEnabled = true;
    }

    internal void UpdateScreenNumber(string screenNumber)
    {
        ScreenNumber = screenNumber;
    }
}
