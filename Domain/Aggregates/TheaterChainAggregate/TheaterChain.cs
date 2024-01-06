using Domain.Aggregates.Exceptions;
using Domain.Aggregates.ShowtimeAggregate;
using Domain.Aggregates.TheaterAggregate;
using System.ComponentModel.DataAnnotations;

namespace Domain.Aggregates.TheaterChainAggregate;

public class TheaterChain
{
    public int Id { get; private set; }

    [Required]
    [StringLength(100, ErrorMessage = "Theater chain name length can't be more than 100 characters.")]
    public string Name { get; private set; }

    [Required]
    [StringLength(300, ErrorMessage = "Theater chain description length can't be more than 300 characters.")]
    public string Description { get; private set; }

    // Navigation properties
    internal List<Theater> Theaters { get; private set; }
    internal List<Movie> Movies { get; private set; }

    public TheaterChain(int id, string name, string description, List<Movie> movies)
    {
        Id = id;
        Name = name;
        Description = description;
        Theaters = [];
        Movies = [.. movies];
    }

    public void AddTheater(string name, string location)
    {
        Theater? existingTheater = Theaters.Find(t => t.Name == name);
        if (existingTheater != null)
        {
            throw new MovieChainException("Theater already exists");
        }

        existingTheater = Theaters.Find(t => t.Location == location);
        if (existingTheater != null)
        {
            throw new MovieChainException("Theater already exists");
        }

        Theater theater = new(0, name, location, Movies);
        Theaters.Add(theater);
    }

    public void RemoveTheater(int theaterId)
    {
        Theater? theater = Theaters.Find(t => t.Id == theaterId);
        if (theater != null)
        {
            IEnumerable<Showtime> showtimes = theater.GetActiveShowtimes();

            if (showtimes.Any())
            {
                throw new MovieChainException("Theater has future showtimes");
            }

            Theaters.Remove(theater);
        }
        else
        {
            throw new MovieChainException("Theater does not exist");
        }
    }

    public void UpdateTheater(int id, string updatedName, string updatedLocation)
    {
        Theater? theater = Theaters.Find(t => t.Id == id);
        if (theater != null)
        {
            theater.UpdateInformation(updatedName, updatedLocation);
        }
        else
        {
            throw new MovieChainException("Theater does not exist");
        }
    }

    internal void AddMovie(Movie movie)
    {
        Movies.Add(movie);
    }

    public Movie AddMovie(string title, string description, string genre, TimeSpan duration, DateTime releaseDateUtc)
    {
        Movie movie = new(0, title, description, duration, genre, releaseDateUtc);
        Movies.Add(movie);
        return movie;
    }

    public void UpdateMovie(int id, string title, string description, string genre, TimeSpan duration, DateTime releaseDateUtc)
    {
        Movie? movie = Movies.Find(m => m.Id == id);
        if (movie != null)
        {
            movie.UpdateInformation(title, description, genre, duration, releaseDateUtc);
        }
        else
        {
            throw new MovieChainException("Movie does not exist");
        }
    }

    public void MarkMovieAsNoLongerAvailable(int id)
    {
        Movie? movie = Movies.Find(m => m.Id == id);
        if (movie != null)
        {
            bool moveHasFutureShowtimes = HasMovieGotAnyFutureShowtimes(movie);
            if (moveHasFutureShowtimes)
            {
                throw new MovieChainException("Movie has future showtimes");
            }
            movie.MarkAsNoLongerAvailable();
        }
        else
        {
            throw new MovieChainException("Movie does not exist");
        }
    }

    public void MarkMovieAsAvailable(int id)
    {
        Movie? movie = Movies.Find(m => m.Id == id);
        if (movie != null)
        {
            movie.MarkAsAvailable();
        }
        else
        {
            throw new MovieChainException("Movie does not exist");
        }
    }

    private bool HasMovieGotAnyFutureShowtimes(Movie movie)
    => Theaters.Any(t => t.HasMovieGotAnyFutureShowtimes(movie.Id));

    public List<Movie> GetMovies()
        => Movies;
}