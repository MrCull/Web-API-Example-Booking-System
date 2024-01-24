using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;
using Domain.Exceptions;
using FluentAssertions;

namespace Domain.Test.Aggregates.TheaterChainAggregate;

[TestFixture]
public class TheaterChainTests
{
    private TheaterChain _theaterChain;
    private Theater _theater;
    private Movie _movie;

    [SetUp]
    public void Setup()
    {
        _movie = new Movie(0, "Movie A", "Description A", 136, "Action", new DateTime(2022, 1, 1));
        List<Movie> movies = [_movie];

        _theaterChain = new TheaterChain(1, "Chain A", "Description A", movies);

        _theaterChain.AddTheater("Theater A", "Location A");
        _theater = _theaterChain.Theaters.Single();
    }

    #region Theaters

    [Test]
    /// As a Theater Chain Manager,
    /// I want to update a theater's details in our theater chain,
    /// So that our theater information is always current and accurate for customers.
    /// AC: The system allows for updating the details of theaters in the theater chain.
    public void AddTheater_ToTheaterChain_TheaterIsAdded()
    {
        // Act
        _theaterChain.AddTheater("New Theater Name", "New Location B");

        // Assert
        _theaterChain.Theaters.Count.Should().Be(2);

        _theaterChain.Theaters.Skip(1).Single().Name.Should().Be("New Theater Name");
        _theaterChain.Theaters.Skip(1).Single().Location.Should().Be("New Location B");
    }

    [Test]
    /// As a Theater Chain Manager,
    /// I want to update a theater's details in our theater chain,
    /// So that our theater information is always current and accurate for customers.
    /// AC: The system allows for updating the details of theaters in the theater chain.
    public void UpdateTheater_Details_TheaterDetailsAreUpdated()
    {
        // Arrange
        string updatedName = "Updated Theater A";
        string updatedLocation = "Updated Location A";

        // Act
        _theaterChain.UpdateTheater(_theater.Id, updatedName, updatedLocation);

        // Assert
        ITheater? updatedTheater = _theaterChain.Theaters.FirstOrDefault(t => t.Id == _theater.Id);
        updatedTheater.Should().NotBeNull();
        updatedTheater!.Name.Should().Be(updatedName);
        updatedTheater.Location.Should().Be(updatedLocation);
    }

    [Test]
    /// As a Theater Chain Manager,
    /// I want to ensure that updating a non-existent theater's details has no effect,
    /// So that the integrity of our theater data is maintained.
    /// AC: MovieChainException thrown with message "Theater does not exist"
    public void UpdateTheater_InvalidTheaterId_NoUpdateOccurs()
    {
        // Arrange
        const int nonExistentTheaterId = -1; // assuming -1 is an invalid ID

        // Act & Assert
        MovieChainException? exception = Assert.Throws<MovieChainException>(() => _theaterChain.UpdateTheater(nonExistentTheaterId, "Updated Name", "Updated Location"));
        exception.Message.Should().Be("Theater does not exist");
    }

    [Test]
    /// As a Theater Chain Manager,
    /// I want to ensure theaters with future showtimes are not removed,
    /// So that we can maintain a reliable schedule for our customers.
    /// AC: MovieChainException thrown with message "Theater has future showtimes"
    public void RemoveTheater_WithFutureShowtimes_TheaterNotRemoved()
    {
        // Arrange
        _theater.AddScreen("1");

        IScreen _screen = _theater.GetScreenByName("1");

        _theater.AddShowtime(DateTime.UtcNow.AddDays(1), 9.99m, _screen.Id, _movie.Id);

        // Act & Assert
        MovieChainException? exception = Assert.Throws<MovieChainException>(() => _theaterChain.RemoveTheater(_theater.Id));
        exception.Message.Should().Be("Theater has future showtimes");
    }

    [Test]
    /// As a Theater Chain Manager,
    /// I want to remove theaters without future showtimes,
    /// So that our theater listings are always up to date and reflective of our current offerings.
    /// AC: Theaters without future showtimes can be removed from the theater chain.
    public void RemoveTheater_WithoutFutureShowtimes_TheaterIsRemoved()
    {
        // Act
        _theaterChain.RemoveTheater(_theater.Id);

        // Assert
        _theaterChain.Theaters.Should().NotContain(_theater);
    }

    [Test]
    /// As a Theater Chain Manager,
    /// I want to prevent duplicate theaters from being added,
    /// So that we avoid confusion and maintain a clear and concise theater listing.
    /// AC: MovieChainException thrown with message "Theater already exists"
    public void AddTheater_DuplicateTheater_TheaterNotAdded()
    {
        // Act & Assert
        MovieChainException? exception = Assert.Throws<MovieChainException>(() => _theaterChain.AddTheater(_theater.Name, _theater.Location));
        exception.Message.Should().Be("Theater already exists");
    }

    #endregion

    #region Movies

    [Test]
    /// As a Theater Chain Manager,
    /// I want to manage the list of movies that are available across our theater chain,
    /// So that we can offer a diverse and appealing selection to our customers.
    /// AC: The system allows adding new movies to the theater chain.
    public void AddMovie_ToTheaterChain_MovieIsAdded()
    {
        // Act
        IMovie movie = _theaterChain.AddMovie("Movie B", "Description B", "Horror", 70, new DateTime(2000, 1, 1));

        // Assert
        _theaterChain.Movies.Count.Should().Be(2);

        movie.Title.Should().Be("Movie B");
        movie.Description.Should().Be("Description B");
        movie.Genre.Should().Be("Horror");
        movie.DurationMins.Should().Be(70);
        movie.ReleaseDateUtc.Should().Be(new DateTime(2000, 1, 1));
    }

    [Test]
    /// As a Theater Chain Manager,
    /// I want to manage the list of movies that are available across our theater chain,
    /// So that we can remove outdated or underperforming movies, keeping our offerings fresh and relevant.
    /// AC: The system permits the removal of movies in the theater chain.
    public void UpdateMovie_Information_InformationIsUpdated()
    {
        // Arrange
        _theaterChain.AddMovie(_movie);

        // Act
        _theaterChain.UpdateMovie(_movie.Id, "Updated Movie Title", "Updated Description", "Updated Genre", 150, new DateTime(2022, 12, 1));

        // Assert
        IMovie? movieInChain = _theaterChain.Movies.Find(m => m.Id == _movie.Id);
        movieInChain.Should().NotBeNull();
        movieInChain!.Title.Should().Be("Updated Movie Title");
        movieInChain.Description.Should().Be("Updated Description");
        movieInChain.Genre.Should().Be("Updated Genre");
        movieInChain.DurationMins.Should().Be(150);
        movieInChain.ReleaseDateUtc.Should().Be(new DateTime(2022, 12, 1));
    }

    [Test]
    /// As a Theater Chain Manager,
    /// I want to mark a movie as no longer available,
    /// So that it's clear to customers that the movie is not currently being shown, while keeping it in our records.
    /// AC: A movie's status can be updated to 'no longer available', but it cannot be deleted from the system.
    public void MarkMovieAsNoLongerAvailable_AvilableMovie_MovieNoLongerAvailable()
    {
        // Arrange
        _theaterChain.AddMovie(_movie);

        // Act
        _theaterChain.MarkMovieAsNoLongerAvailable(_movie.Id);

        // Assert
        _movie.TheaterChainMovieStatus.Should().Be(TheaterChainMovieStatus.NoLongerAvailable);
    }


    /// As a Theater Chain Manager,
    /// I want to ensure that movies with future showtimes are not marked as unavailable,
    /// So that we don't disrupt scheduled showtimes for our customers.
    /// AC: MovieChainException thrown with message "Movie has future showtimes"
    [Test]
    public void MarkMovieAsNoLongerAvailable_WithFutureShowtimes_FailsToUpdate()
    {
        // Arrange
        _theater.AddScreen("1");

        IScreen screen = _theater.GetScreenByName("1");

        _theater.AddShowtime(DateTime.Now.AddDays(1), 9.99m, screen.Id, _movie.Id);

        // Act & Assert
        MovieChainException? movieChainException = Assert.Throws<MovieChainException>(() => _theaterChain.MarkMovieAsNoLongerAvailable(_movie.Id));
        _movie.TheaterChainMovieStatus.Should().Be(TheaterChainMovieStatus.Available);

        movieChainException.Message.Should().Be("Movie has future showtimes");
    }

    /// As a Theater Chain Manager,
    /// I want to ensure that updating a non-existent movie's status has no effect,
    /// So that the integrity of our movie data is maintained.
    /// AC: MovieChainException thrown with message "Movie does not exist"
    [Test]
    public void MarkMovieAsNoLongerAvailable_InvalidMovieId_FailsToUpdate()
    {
        // Act & Assert
        MovieChainException? movieChainException = Assert.Throws<MovieChainException>(() => _theaterChain.MarkMovieAsNoLongerAvailable(-1));
        movieChainException.Message.Should().Be("Movie does not exist");

        _movie.TheaterChainMovieStatus.Should().Be(TheaterChainMovieStatus.Available);
    }

    [Test]
    /// As a Theater Chain Manager,
    /// I want to mark a movie as available,
    /// So that old movies can be brought back into circulation if they become relevant again.
    /// AC: A movie's status can be updated back to 'available'.
    public void MarkMovieAsAvailable_NoLongerAvailableMovie_MovieIsAvailable()
    {
        // Arrange
        _movie.MarkAsNoLongerAvailable();

        // Act
        _theaterChain.MarkMovieAsAvailable(_movie.Id);

        // Assert
        _movie.TheaterChainMovieStatus.Should().Be(TheaterChainMovieStatus.Available);
    }

    [Test]
    /// As a Theater Chain Manager,
    /// I want to get a list of all movies in our theater chain,
    /// So that I can see what movies are currently available.
    /// Ac: The system allows for listing all movies in the theater chain.
    public void GetMovies_AllMoviesInChain_ListOfMoviesReturned()
    {
        // Arrange
        // One already exists from the setup, and 2 more will be added here.
        _theaterChain.AddMovie("Movie A", "Description A", "Action", 136, new DateTime(2022, 1, 1));
        _theaterChain.AddMovie("Movie B", "Description B", "Horror", 70, new DateTime(2000, 1, 1));

        // Act
        List<IMovie> movies = _theaterChain.GetMovies();

        // Assert
        movies.Count.Should().Be(3);
        movies.Should().Contain(_movie);
    }

    [Test]
    /// As a Theater Chain Manager,
    /// I want to get a movie by its ID,
    /// So that I can see its details.
    /// AC: The system allows for getting a movie by its ID.
    public void GetMovieById_MovieExists_MovieReturned()
    {
        // Act
        IMovie? movie = _theaterChain.GetMovieById(_movie.Id);

        // Assert
        movie.Should().NotBeNull();
        movie!.Should().Be(_movie);
    }

    [Test]
    /// As a Theater Chain Manager,
    /// I want to ensure that getting a non-existent movie by its ID has no effect,
    /// So that the integrity of our movie data is maintained.
    /// AC: MovieChainException thrown with message "Movie with [{id}] does not exist"
    public void GetMovieById_InvalidMovieId_FailsToGetMovie()
    {
        // Act & Assert
        MovieChainException? movieChainException = Assert.Throws<MovieChainException>(() => _theaterChain.GetMovieById(-1));
        movieChainException.Message.Should().Be("Movie with id [-1] does not exist");
    }
    #endregion
}
