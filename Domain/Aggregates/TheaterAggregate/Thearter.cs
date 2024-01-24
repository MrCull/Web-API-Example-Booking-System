using Domain.Aggregates.ShowtimeAggregate;
using Domain.Aggregates.TheaterChainAggregate;
using Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Domain.Aggregates.TheaterAggregate;

public class Theater : IAggregrateRoot, ITheater
{
    public int Id { get; private set; }

    [Required]
    [StringLength(100, ErrorMessage = "Theater name length can't be more than 100 characters.")]
    public string Name { get; private set; }

    [Required]
    [StringLength(200, ErrorMessage = "Location length can't be more than 200 characters.")]
    public string Location { get; private set; }

    // Navigation property for screens
    internal readonly List<Movie> Movies = [];

    internal readonly List<Screen> Screens = [];

    internal readonly List<Showtime> Showtimes = [];

    internal Theater(int id, string name, string location, List<Movie> movies, List<Screen> screens, List<Showtime> showtimes)
    {
        Id = id;
        Name = name;
        Location = location;
        Movies.AddRange(movies);
        Screens = screens;
        Showtimes = showtimes;
    }

    internal Theater(int id, string name, string location, List<Movie> movies)
        : this(id, name, location, movies, [], [])
    {

    }

    public void AddScreen(string screenNumber)
    {
        IScreen? existingScreen = Screens.Find(s => s.ScreenNumber == screenNumber);
        if (existingScreen is not null)
        {
            throw new TheaterException("Screen already exists");
        }

        Screen screen = new(Id, screenNumber);
        Screens.Add(screen);
    }

    public void AddShowtime(DateTime dateTime, decimal price, Guid screenId, int movieId)
    {
        ValidateInputAndThrowIfInvalid(dateTime, price);

        Movie? movie = Movies.Find(m => m.Id == movieId);
        Screen? screen = Screens.Find(s => s.Id == screenId);

        Showtime showtime = new(0, movie, screen, dateTime, price);

        screen.AddShowtime(showtime);

        Showtimes.Add(showtime);
    }

    private void ValidateInputAndThrowIfInvalid(DateTime showDateTimeUtc, decimal price)
    {
        if (showDateTimeUtc < DateTime.UtcNow) throw new TheaterException("Showtime is in the past");

        if (showDateTimeUtc > DateTime.UtcNow.AddYears(1)) throw new TheaterException("Showtime is more than 1 year in the future");

        if (price < 0) throw new TheaterException("Price cannot be less than 0");

        if (Showtimes.Exists(_ => showDateTimeUtc >= _.ShowDateTimeUtc && showDateTimeUtc <= _.ShowDateTimeUtc.AddMinutes(_.Movie.DurationMins)))
        {
            throw new TheaterException("Screen already has a showtime scheduled for this date and time");
        }

        if (Showtimes.Exists(_ => showDateTimeUtc >= _.ShowDateTimeUtc && showDateTimeUtc <= _.ShowDateTimeUtc.AddMinutes(_.Movie.DurationMins).AddMinutes(50)))
        {
            throw new TheaterException("Screen needs at least 50mins before next Showtime");
        }
    }

    public Screen GetScreenByName(string screenName)
    {
        Screen? screen = Screens.Find(s => s.ScreenNumber == screenName);
        if (screen is not null)
        {
            return screen;
        }
        else
        {
            throw new TheaterException("Screen does not exist");
        }
    }

    internal List<Screen> GetActiveScreens()
        => Screens.Where(s => s.IsEnabled).ToList();

    public void ReenableScreen(Guid id)
    {
        Screen? screen = Screens.Find(s => s.Id == id);
        if (screen is not null)
        {
            screen.Reenable();
        }
        else
        {
            throw new TheaterException("Screen does not exist");
        }
    }

    public void DisableScreen(Guid screenId)
    {
        Screen? screen = Screens.Find(s => s.Id == screenId);
        if (screen is not null)
        {
            if (screen.HasFutureShowtimes())
            {
                throw new TheaterException("Screen has future showtimes");
            }

            screen.Disable();
        }
        else
        {
            throw new TheaterException("Screen does not exist");
        }
    }

    public void RemoveShowtime(int id)
    {
        Showtime? showtime = Showtimes.Find(s => s.Id == id);
        if (showtime is not null)
        {
            if (showtime.HasActiveSeatReservations())
            {
                throw new TheaterException("Showtime has active reservations");
            }

            Showtimes.Remove(showtime);
        }
        else
        {
            throw new TheaterException("Showtime does not exist");
        }
    }

    public void UpdateInformation(string newName, string newLocation)
    {
        Name = newName;
        Location = newLocation;
    }

    public void UpdateScreen(Guid id, string name)
    {
        Screen? screen = Screens.Find(s => s.Id == id);
        if (screen is not null)
        {
            screen.UpdateScreenNumber(name);
        }
        else
        {
            throw new TheaterException("Screen does not exist");
        }
    }

    internal IEnumerable<Showtime> GetActiveShowtimes()
        => Showtimes.Where(s => s.ShowDateTimeUtc > DateTime.UtcNow).
        OrderBy(s => s.ShowDateTimeUtc)
        .ToList();

    public void UpdateShowtime(int id, DateTime newDateTime, decimal newPrice, Guid screenId)
    {
        Showtime? showtime = Showtimes.Find(s => s.Id == id);

        if (showtime is not null)
        {
            if (showtime.HasActiveSeatReservations())
            {
                throw new TheaterException("Showtime has active reservations");
            }

            Screen? screen = Screens.Find(s => s.Id == screenId);
            if (screen is not null)
            {
                showtime.UpdateInformation(newDateTime, newPrice, screen);
            }
            else
            {
                throw new TheaterException("Screen does not exist");
            }
        }
        else
        {
            throw new TheaterException("Showtime does not exist");
        }
    }

    public IEnumerable<IMovie> GetMoviesWithActiveShowtimes()
    {
        IEnumerable<int> activeMovieIds = Showtimes
            .Where(_ => _.ShowDateTimeUtc > DateTime.UtcNow)
            .Select(_ => _.Movie.Id)
            .Distinct();

        return Movies.Where(_ => activeMovieIds.Contains(_.Id));
    }

    internal void ClearSeatReservationsWithExpiredTimeouts()
      => Showtimes.ForEach(_ => _.ClearSeatReservationsWithExpiredTimeouts());


    public bool HasMovieGotAnyFutureShowtimes(int id)
        => GetActiveShowtimes().Any(s => s.Movie?.Id == id);
}



