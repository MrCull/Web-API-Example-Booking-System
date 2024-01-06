
using Domain.Aggregates.Exceptions;
using Domain.Aggregates.TheaterChainAggregate;
using FluentAssertions;

namespace Domain.Test.Aggregates.TheaterChainAggregate;

internal class MovieTests
{
    private Movie _movie;

    [SetUp]
    public void Setup()
    {
        _movie = new Movie(1, "Movie Title", "Description", TimeSpan.FromMinutes(120), "Genre", new DateTime(2022, 1, 1));
    }

    [Test]
    public void UpdateInformation_NewInfo_InformationIsUpdated()
    {
        // Arrange
        Movie updatedMovie = new(1, "Updated Movie Title", "Updated Description", TimeSpan.FromMinutes(1), "Updated Genre", new DateTime(1966, 1, 1));

        // Act
        _movie.UpdateInformation(updatedMovie.Title, updatedMovie.Description, updatedMovie.Genre, updatedMovie.Duration, updatedMovie.ReleaseDateUtc);

        // Assert
        _movie.Should().BeEquivalentTo(updatedMovie);
    }

    [Test]
    public void MarkAsNoLongerAvailable_MovieIsNoLongerAvailable()
    {
        // Act
        _movie.MarkAsNoLongerAvailable();

        // Assert
        _movie.TheaterChainMovieStatus.Should().Be(TheaterChainMovieStatus.NoLongerAvailable);
    }

    [Test]
    /// AC: Specific exception with message "Movie is already no longer available"
    public void MarkAsNoLongerAvailable_MovieIsAlreadyNoLongerAvailable_ExceptionRaised()
    {
        // Arrange
        _movie.MarkAsNoLongerAvailable();

        // Act & Assert
        MovieException? exception = Assert.Throws<MovieException>(() => _movie.MarkAsNoLongerAvailable());
        exception.Message.Should().Be("Movie is already no longer available");
    }

    [Test]
    public void MarkAsAvailable_CurrentlyUnavilable_MovieIsAvailable()
    {
        // Arrange
        _movie.MarkAsNoLongerAvailable();

        // Act
        _movie.MarkAsAvailable();

        // Assert
        _movie.TheaterChainMovieStatus.Should().Be(TheaterChainMovieStatus.Available);
    }

    [Test]
    /// AC: Specific exception with message "Movie is already available"
    public void MarkAsAvailable_MovieIsAlreadyAvailable_ExceptionRaised()
    {
        // Act & Assert
        MovieException? exception = Assert.Throws<MovieException>(() => _movie.MarkAsAvailable());
        exception.Message.Should().Be("Movie is already available");
    }
}