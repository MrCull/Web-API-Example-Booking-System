using Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Aggregates.TheaterChainAggregate;

internal class Movie : IMovie
{
    [JsonConstructor]
    public Movie(int id, string title, string description, int durationMins, string genre, DateTime releaseDateUtc, TheaterChainMovieStatus movieStatus = TheaterChainMovieStatus.Available)
    {
        Id = id;
        Title = title;
        Description = description;
        DurationMins = durationMins;
        Genre = genre;
        ReleaseDateUtc = releaseDateUtc;
        TheaterChainMovieStatus = movieStatus;
    }

    public int Id { get; private set; }

    [Required]
    [StringLength(100, ErrorMessage = "Title length can't be more than 100 characters.")]
    public string Title { get; private set; }

    [Required]
    [StringLength(500, ErrorMessage = "Description length can't be more than 500 characters.")]
    public string Description { get; private set; }

    [Range(1, 1440, ErrorMessage = "Only positive number allowed and up to 1 day in total")]
    [Required]
    public int DurationMins { get; private set; }

    [Required]
    [StringLength(50, ErrorMessage = "Genre length can't be more than 50 characters.")]
    public string Genre { get; private set; }

    [DataType(DataType.Date)]
    public DateTime ReleaseDateUtc { get; private set; }

    [Required]
    public TheaterChainMovieStatus TheaterChainMovieStatus { get; private set; }

    public void MarkAsAvailable()
    {
        if (TheaterChainMovieStatus == TheaterChainMovieStatus.Available)
        {
            throw new MovieException("Movie is already available");
        }

        TheaterChainMovieStatus = TheaterChainMovieStatus.Available;
    }

    public void MarkAsNoLongerAvailable()
    {
        if (TheaterChainMovieStatus == TheaterChainMovieStatus.NoLongerAvailable)
        {
            throw new MovieException("Movie is already no longer available");
        }

        TheaterChainMovieStatus = TheaterChainMovieStatus.NoLongerAvailable;
    }

    internal void UpdateInformation(string title, string description, string genre, int durationMins, DateTime releaseDate)
    {
        Title = title;
        Description = description;
        Genre = genre;
        DurationMins = durationMins;
        ReleaseDateUtc = releaseDate;
    }
}

public enum TheaterChainMovieStatus
{
    Available,
    NoLongerAvailable
}