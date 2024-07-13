using Domain.Aggregates.ShowtimeAggregate;
using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;
using Domain.Exceptions;
using FluentAssertions;

namespace Domain.Test.Aggregates.TheaterAggregate;

internal class TheaterTests
{
    private Theater _theater;
    private Movie _movie;


    [SetUp]
    public void Setup()
    {
        _movie = new Movie(1, "Movie A", "Description A", 136, "Action", new DateTime(2022, 1, 1));
        List<Movie> movies = [_movie];

        _theater = new Theater(1, "Theater A", "Location A", movies);
    }

    #region Theater Management Tests

    [Test]
    /// As a Theater Manager,
    /// I want to update theater information,
    /// So that the theater's details are accurate and up-to-date.
    /// AC: Theater information is updated.
    public void UpdateInformation_NewInformation_InformationIsUpdated()
    {
        // Arrange
        string newName = "New Theater Name";
        string newLocation = "New Location";

        // Act
        _theater.UpdateInformation(newName, newLocation);

        // Assert
        _theater.Name.Should().Be(newName, "because the theater's name should have been updated");
        _theater.Location.Should().Be(newLocation, "because the theater's location should have been updated");
    }

    #endregion

    #region Theater Management Screen Tests

    [Test]
    /// As a Theater Manager,
    /// I want to update screen details,
    /// So that the screen's information is accurate and current.
    /// AC: Screen details are updated in the theater.
    public void AddScreen_ToTheater_ScreenIsAdded()
    {
        // Act
        _theater.AddScreen("1");

        // Assert
        _theater.Screens.Count.Should().Be(1);
        _theater.Screens.Single().ScreenNumber.Should().Be("1");
    }

    [Test]
    /// As a Theater Manager,
    /// I want to prevent the addition of duplicate screens,
    /// So that the theater layout remains organized and clear.
    /// AC: Specific exception with message "Screen already exists"
    public void AddScreen_ToTheater_DuplicateNotAdded()
    {
        // Arrange
        _theater.AddScreen("1");

        // Act & Assert
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.AddScreen("1"));
        exception.Message.Should().Be("Screen already exists");
        _theater.Screens.Count.Should().Be(1, "because the duplicate screen should not have been added");
    }

    [Test]
    /// As a Theater Manager,
    /// I want to disable screens in a theater,
    /// So that so these are not avilable for new showtimes.
    /// AC: Screen is disabled.
    public void DisableScreen_FromTheater_ScreenIsRemovedFromActiveScreens()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");

        // Act
        _theater.DisableScreen(screen.Id);

        // Assert
        _theater.GetActiveScreens().Should().NotContain(screen);
    }

    [Test]
    /// As a Theater Manager,
    /// I want to ensure non-existent screens cannot be removed,
    /// So that the theater's data integrity is maintained.
    /// AC: Specific exception with message "Screen does not exist"
    public void DisableScreen_ScreenDoesNotExists_SpecificExceptionRaised()
    {
        // Act & Assert
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.DisableScreen(Guid.NewGuid()));
        exception.Message.Should().Be("Screen does not exist");
    }

    [Test]
    /// As a Theater Manager,
    /// I want to reenable a screen that was previously removed,
    /// So that we can start using the screen again.
    /// AC: Screen is reenabled.
    public void ReenableScreen_ScreenIsDisabled_ScreenIsReenabled()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");
        _theater.DisableScreen(screen.Id);

        // Act
        _theater.ReenableScreen(screen.Id);

        // Assert
        _theater.Screens.Single().IsEnabled.Should().BeTrue();
    }

    [Test]
    /// As a Theater Manager,
    /// I want to ensure that updating a non-existent screen within the theater has no effect,
    /// So that our theater's screen data remains consistent and accurate.
    /// AC: Specific exception with message "Screen does not exist"
    public void UpdateScreen_InvalidId_SpecificExceptionRaisedAndNoUpdateOccurs()
    {
        // Act & Assert
        Guid screenId = Guid.NewGuid();
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.UpdateScreen(screenId, "Updated Screen Number"));
        exception.Message.Should().Be($"Screen with id[{screenId}] does not exist");
    }

    [Test]
    /// As a Theater Manager,
    /// I want to ensure that a screen currently in use by a scheduled showtime cannot be removed,
    /// So that we do not disrupt scheduled movies and customers' plans.
    /// AC: Specific exception with message "Screen is in use by a future showtime"
    public void DisableScreen_InUseByShowtime_ThrowsException()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");

        _theater.AddShowtime(DateTime.Now.AddDays(1), 9.99m, screen.Id, _movie.Id);

        // Act & Assert
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.DisableScreen(screen.Id));
        exception.Message.Should().Be("Screen has future showtimes");
    }

    /// AC: Existing enabled screen returned
    [Test]
    public void GetScreen_ScreenExists_ScreenReturned()
    {
        // Arrange
        _theater.AddScreen("1");

        // Act
        IScreen screen = _theater.GetScreenByName("1");

        // Assert
        screen.ScreenNumber.Should().Be("1");
    }

    [Test]
    /// AC: Existing disabled screen returned
    public void GetScreenByName_ScreenExistsButDisabled_ScreenReturned()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");
        _theater.DisableScreen(screen.Id);

        // Act
        Screen disabledScreen = _theater.GetScreenByName("1");

        // Assert
        disabledScreen.ScreenNumber.Should().Be("1");

        disabledScreen.IsEnabled.Should().Be(false);
    }

    [Test]
    /// AC: Excption with message "Screen does not exist"
    public void GetScreenByName_ScreenDoesNotExist_ExceptionRaised()
    {
        // Act
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.GetScreenByName("1"));
        exception.Message.Should().Be("Screen with name[1] does not exist");
    }

    [Test]
    /// As a Theater Manager,
    /// I want only enabled screens to be available for scheduling,
    /// So that only screens that are currently in use are available for scheduling.
    /// AC: Only enabled screens are returned.
    public void GetActiveScreens_ThreeScreensTwoDisabled_TwoScreensReturned()
    {
        // Arrange
        _theater.AddScreen("1");
        _theater.AddScreen("2");
        _theater.AddScreen("3");

        Screen screen1 = _theater.GetScreenByName("1");
        Screen screen2 = _theater.GetScreenByName("2");
        Screen screen3 = _theater.GetScreenByName("3");

        _theater.DisableScreen(screen1.Id);
        _theater.DisableScreen(screen2.Id);

        // Act
        List<Screen> screens = _theater.GetActiveScreens();

        // Assert
        screens.Should().Contain(screen3);
        screens.Should().NotContain(screen1);
        screens.Should().NotContain(screen2);
        screens.Count.Should().Be(1);
    }

    #endregion Theater Management Screen Tests

    #region Theater Management Showtime Tests

    [Test]
    /// As a Theater Manager,
    /// I want to remove future showtimes from a screen,
    /// So that I can manage scheduling effectively.
    /// AC: Future showtime is removed from the screen.
    public void RemoveShowtime_FromScreen_ShowtimeIsRemoved()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");
        _theater.AddShowtime(DateTime.Now.AddDays(1), 9.99m, screen.Id, _movie.Id);

        IShowtime showtime = _theater.GetActiveShowtimes().Single();

        // Act
        _theater.RemoveShowtime(showtime.Id);

        // Assert
        _theater.GetActiveShowtimes().Should().NotContain(showtime, "because the showtime should be removed from the screen");
    }

    [Test]
    /// As a Theater Manager,
    /// I want to prevent adding invalid showtimes
    /// So that the theater's schedule remains feasible and reliable.
    /// AC: Exception raised with message "Showtime is in the past"
    public void AddShowtime_InThePast_ThrowsException()
    {
        // Arrange
        DateTime pastDateTime = DateTime.Now.AddDays(-1);
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");

        // Act & Assert
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.AddShowtime(pastDateTime, 9.99m, screen.Id, _movie.Id));
        exception.Message.Should().Be("Showtime is in the past");
    }

    [Test]
    /// As a Theater Manager,
    /// I want to prevent adding invalid showtimes
    /// So that the theater's schedule remains feasible and reliable.
    /// AC: Exception raised with message "Showtime is more than 1 year in the future"
    public void AddShowtime_MoreThanOneYearInFuture_ThrowsException()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");
        DateTime futureDateTime = DateTime.Now.AddYears(1).AddDays(1);

        // Act & Assert
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.AddShowtime(futureDateTime, 9.99m, screen.Id, _movie.Id));
        exception.Message.Should().Be("Showtime is more than 1 year in the future");
    }

    [Test]
    /// As a Theater Manager,
    /// I want to prevent allowing minus prices for showtimes,
    /// So that the theater's pricing is not stupid.
    /// AC: Exception raised with message "Price cannot be less than 0"
    public void AddShowtime_WithMinusPrice_ThrowsException()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");
        DateTime futureDateTime = DateTime.Now.AddDays(1);

        // Act & Assert
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.AddShowtime(futureDateTime, -1, screen.Id, _movie.Id));
        exception.Message.Should().Be("Price cannot be less than 0");
    }

    [Test]
    /// As a Theater Manager,
    /// I want to prevent double booking of showtimes on the same screen,
    /// So that there are no scheduling conflicts and customers have a clear and reliable schedule.
    /// AC: Same start time now allowed. Exception raised "Screen already has a showtime scheduled for this date and time"
    public void AddShowtime_ToScreenWithExistingShowtimeWithSameDate_ThrowsException()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");
        DateTime futureDateTime = DateTime.Now.AddDays(1);
        _theater.AddShowtime(futureDateTime, 9.99m, screen.Id, _movie.Id);

        // Act & Assert
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.AddShowtime(futureDateTime, 9.99m, screen.Id, _movie.Id));
        exception.Message.Should().Be("Screen already has a showtime scheduled for this date and time");
    }

    [Test]
    /// As a Theater Manager,
    /// I want to prevent double booking of showtimes on the same screen,
    /// So that there are no scheduling conflicts and customers have a clear and reliable schedule.
    /// AC: Overlapping runtimes now allowed. Exception raised "Screen already has a showtime scheduled for this date and time"
    public void AddShowtime_ToScreenWithExistingShowtimeOverlappingRuntime_ThrowsException()
    {
        // Arrange
        _movie.UpdateInformation(_movie.Title, _movie.Description, _movie.Genre, 120, _movie.ReleaseDateUtc);
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");
        DateTime futureDateTime = DateTime.Now.AddDays(1);
        _theater.AddShowtime(futureDateTime, 9.99m, screen.Id, _movie.Id);

        // Act & Assert
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.AddShowtime(futureDateTime.AddMinutes(119), 9.99m, screen.Id, _movie.Id));
        exception.Message.Should().Be("Screen already has a showtime scheduled for this date and time");
    }

    [Test]
    /// As a Theater Manager,
    /// I want to make sure a screen has a buffer of 50 minutes between showtimes,
    /// So that we have time to clean the theater and prepare for the next showtime.
    /// AC: Buffer of 50 minutes enforced. Exception raised "Screen needs at least 50mins before next Showtime"
    public void AddShowtime_ToScreenWithExistingShowtimeLessThan50MinutesBetween_ThrowsException()
    {
        // Arrange
        _movie.UpdateInformation(_movie.Title, _movie.Description, _movie.Genre, 120, _movie.ReleaseDateUtc);
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");
        DateTime futureDateTime = DateTime.Now.AddDays(1);
        _theater.AddShowtime(futureDateTime, 9.99m, screen.Id, _movie.Id);

        DateTime futureDateTimeWithNotEnoughBufferTime = futureDateTime.AddMinutes(_movie.DurationMins).AddMinutes(49);

        // Act & Assert
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.AddShowtime(futureDateTime.AddMinutes(_movie.DurationMins).AddMinutes(49), 9.99m, screen.Id, _movie.Id));
        exception.Message.Should().Be("Screen needs at least 50mins before next Showtime");
    }

    [Test]

    /// As a Theater Manager,
    /// I want to make sure a screen has a buffer of 50 minutes between showtimes,
    /// So that we have time to clean the theater and prepare for the next showtime.
    /// AC: When more than 50 minutes between showtimes, showtime is added.
    public void AddShowtime_ToScreenWithExistingShowtimeMoreThan50MinutesBetween_ShowtimeAdded()
    {
        // Arrange
        _movie.UpdateInformation(_movie.Title, _movie.Description, _movie.Genre, 120, _movie.ReleaseDateUtc);
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");
        DateTime futureDateTime = DateTime.Now.AddDays(1);
        _theater.AddShowtime(futureDateTime, 9.99m, screen.Id, _movie.Id);

        DateTime futureDateTimeWithEnoughBufferTime = futureDateTime.AddMinutes(_movie.DurationMins).AddMinutes(51);

        // Act
        _theater.AddShowtime(futureDateTimeWithEnoughBufferTime, 9.99m, screen.Id, _movie.Id);

        // Assert
        _theater.GetActiveShowtimes().Count().Should().Be(2);
    }

    [Test]
    /// As a Theater Manager,
    /// I want to update showtime details,
    /// So that the schedule reflects accurate and current information.
    /// AC: Showtime details are updated.
    public void UpdateShowtime_Details_ShowtimeDetailsAreUpdated()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");
        DateTime futureDateTime = DateTime.Now.AddDays(1);
        _theater.AddShowtime(futureDateTime, 9.99m, screen.Id, _movie.Id);

        IShowtime showtime = _theater.GetActiveShowtimes().Single();

        DateTime newDateTime = DateTime.Now.AddDays(2);
        decimal newPrice = 19.99m;
        _theater.AddScreen("2");
        Screen newScreen = _theater.GetScreenByName("2");

        // Act
        _theater.UpdateShowtime(showtime.Id, newDateTime, newPrice, newScreen.Id);

        // Assert
        IShowtime updatedShowtime = _theater.GetActiveShowtimes().Single();
        updatedShowtime.ShowDateTimeUtc.Should().Be(newDateTime);
        updatedShowtime.Price.Should().Be(newPrice);
        updatedShowtime.ScreenId.Should().Be(newScreen.Id);
    }

    [Test]
    /// As a Theater Manager,
    /// I want to ensure showtimes with seat reservations cannot be removed,
    /// So that we honor commitments to our customers.
    /// AC: Exceptio raised with message "Showtime has Seat Reservations"
    public void RemoveShowtime_WithActiveSeatReservations_ThrowsException()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");
        DateTime futureDateTime = DateTime.Now.AddDays(1);
        _theater.AddShowtime(futureDateTime, 9.99m, screen.Id, _movie.Id);
        IShowtime showtime = _theater.GetActiveShowtimes().Single();

        List<string> seats = ["A1", "A2"];
        screen.AddSeats(seats);

        showtime.ProvisionallyReserveSeatsAndReturnReservation(seats);

        // Act & Assert
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.RemoveShowtime(showtime.Id));
        exception.Message.Should().Be("Showtime has active reservations");
    }

    [Test]
    /// As a Theater Manager,
    /// I want to ensure showtimes with active confirmed bookings cannot be removed,
    /// So that we honor commitments to our customers.
    /// AC: Exceptio raised with message "Showtime has Seat Reservations"
    public void RemoveShowtime_WithActiveConfirmedSeatReservations_ThrowsException()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");
        DateTime futureDateTime = DateTime.Now.AddDays(1);
        _theater.AddShowtime(futureDateTime, 9.99m, screen.Id, _movie.Id);
        IShowtime showtime = _theater.GetActiveShowtimes().Single();

        List<string> seats = ["A1", "A2"];
        screen.AddSeats(seats);

        ISeatReservation seatReservation = showtime.ProvisionallyReserveSeatsAndReturnReservation(seats);
        showtime.CompleteBookingForSeatReservationAndReturnBooking(seatReservation.Id);

        // Act & Assert
        TheaterException? exception = Assert.Throws<TheaterException>(() => _theater.RemoveShowtime(showtime.Id));
        exception.Message.Should().Be("Showtime has active reservations");
    }

    #endregion Theater Management Showtime Tests

    #region Theater Guest Tests

    [Test]
    /// As a Guest,
    /// I want to view a list of movies currently showing in the theater,
    /// So that I can choose which movie to watch.
    /// AC: Current movies showing in the theater are listed.
    public void GetMoviesWithActiveShowtimes_ThreeActiveShowtimesForTwoMoviesAndOnePastShowTimeForADifferentMovie_TwoMoviesReturned()
    {
        // Arrange
        Movie movie1 = new(1, "Movie A", "Description A", 136, "Action", new DateTime(2022, 1, 1));
        Movie movie2 = new(2, "Movie B", "Description B", 136, "Action", new DateTime(2022, 1, 1));

        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");

        _theater.Showtimes.Add(new Showtime(1, movie1, screen, DateTime.Now.AddDays(1), 9.99m));
        _theater.Showtimes.Add(new Showtime(2, movie1, screen, DateTime.Now.AddDays(2), 9.99m));
        _theater.Showtimes.Add(new Showtime(3, movie2, screen, DateTime.Now.AddDays(-1), 9.99m));

        // Act
        IEnumerable<IMovie> movies = _theater.GetMoviesWithActiveShowtimes();

        // Assert
        movies.Select(_ => _.Id).Should().Contain(movie1.Id);
        movies.Count().Should().Be(1);
    }

    [Test]
    /// As a Guest,
    /// I want to view a theater's showtimes,
    /// So that I can plan my moviegoing experience.
    /// AC: The theater's showtimes are viewable by guests and ordered by date.
    public void GetActiveShowtimes_TwoActiveShowtimesAndOneExpiredShowtime_TheTwoActiveShowtimesReturnedInCorrectDateOrder()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");

        _theater.Showtimes.Add(new Showtime(1, _movie, screen, DateTime.Now.AddDays(2), 9.99m));
        _theater.Showtimes.Add(new Showtime(2, _movie, screen, DateTime.Now.AddDays(1), 9.99m));
        _theater.Showtimes.Add(new Showtime(3, _movie, screen, DateTime.Now.AddDays(-1), 9.99m));

        // Act
        List<IShowtime> showtimes = _theater.GetActiveShowtimes().ToList();

        // Assert
        showtimes.Count.Should().Be(2);
        showtimes[0].ShowDateTimeUtc.Should().BeBefore(showtimes[1].ShowDateTimeUtc);
    }

    #endregion Theater Guest Tests

    #region Theater Manager Seat Reservation Tests

    [Test]
    /// As a Theater Manager,
    /// I want seat reservations to expire after a certain amount of time,
    /// So that customers cannot hold seats indefinitely.
    public void ClearSeatReservationsWithExpiredTimeouts_TwoReservationsOneExpired_OneReservationsRemoved()
    {
        // Arrange
        _theater.AddScreen("1");
        Screen screen = _theater.GetScreenByName("1");

        _theater.AddShowtime(DateTime.Now.AddDays(1), 9.99m, screen.Id, _movie.Id);
        IShowtime showtime = _theater.GetActiveShowtimes().Single();

        List<string> seats = ["A1", "A2"];
        screen.AddSeats(seats);

        showtime.ProvisionallyReserveSeatsAndReturnReservation(["A1"]);

        ISeatReservation seatReservationToExpire = showtime.ProvisionallyReserveSeatsAndReturnReservation(["A2"]);
        seatReservationToExpire.SetReservationTimeout(DateTime.UtcNow.AddMinutes(-1));

        // Act
        _theater.ClearSeatReservationsWithExpiredTimeouts();

        // Assert
        showtime.GetSeatReservations().Count.Should().Be(1);
    }

    #endregion Theater Manager Seat Reservation Tests
}
