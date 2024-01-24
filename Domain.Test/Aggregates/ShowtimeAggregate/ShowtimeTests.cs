using Domain.Aggregates.ShowtimeAggregate;
using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;
using Domain.Exceptions;
using FluentAssertions;

namespace Domain.Test.Aggregates.ShowtimeAggregate;

internal class ShowtimeTests
{
    private Showtime _showtime;
    private Movie _movie;
    private Screen _screen;

    [SetUp]
    public void Setup()
    {
        _movie = new Movie(1, "Movie Title", "Description", 120, "Genre", new DateTime(2022, 1, 1));
        _screen = new Screen(1, "1");
        _screen.AddSeats(["A1", "A2", "A3", "A4", "A5"]);
        _showtime = new Showtime(1, _movie, _screen, DateTime.UtcNow.AddDays(1), 9.99m);
    }

    [Test]
    /// As a Guest,
    /// I want to try and reserve seats for a showtime,
    /// So that I can make a decision on booking tickets.
    /// AC: Seats reserved.
    public void ProvisionalReserveSeats_SeatsAvilable_SeatsReserved()
    {
        // Arrange
        List<string> seatsToReserve = ["A1", "A2", "A3"];
        DateTime timeBeforeReservationUtc = DateTime.UtcNow;

        // Act
        _showtime.ProvisionallyReserveSeatsAndReturnReservation(seatsToReserve);

        // Assert
        _showtime.AvailableSeats().Should().Be(2);

        List<SeatReservation> seatReservations = _showtime.GetSeatReservations();
        seatReservations.Count.Should().Be(1);

        seatReservations[0].Status.Should().Be(ReservationStatus.Reserved);

        seatReservations[0].Seats.Count.Should().Be(3);

        seatReservations[0].Seats[0].SeatNumber.Should().Be("A1");
        seatReservations[0].Seats[1].SeatNumber.Should().Be("A2");
        seatReservations[0].Seats[2].SeatNumber.Should().Be("A3");

        seatReservations[0].ReservationTimeUtc.Should().BeOnOrAfter(timeBeforeReservationUtc);
        seatReservations[0].ReservationTimeUtc.Should().BeOnOrBefore(DateTime.UtcNow);

        seatReservations[0].ReservationTimeoutUtc.Should().BeOnOrAfter(timeBeforeReservationUtc.AddMinutes(20));
        seatReservations[0].ReservationTimeoutUtc.Should().BeOnOrBefore(DateTime.UtcNow.AddMinutes(20));
    }

    [Test]
    /// As a Guest,
    /// I want my reservation request to be rejected if the seats are taken,
    /// So that I'm not sharing a seat.
    /// AC: Specific exception with message "Seats are no longer available"
    public void ProvisionalReserveSeats_SeatsUnavilable_SpecificExceptionThrown()
    {
        // Arrange
        List<string> seatsAlreadyReserved = ["A1", "A3"];
        _showtime.ProvisionallyReserveSeatsAndReturnReservation(seatsAlreadyReserved);

        List<string> seatsToReserve = ["A1", "A2", "A3"];

        // Act & Assert
        ShowtimeException? exception = Assert.Throws<ShowtimeException>(() => _showtime.ProvisionallyReserveSeatsAndReturnReservation(seatsToReserve));
        exception.Message.Should().Be("Seats no longer available [A1,A3]");
    }

    [Test]
    /// As a Guest,
    /// I want to make my booking with the Seat Reservations,
    /// So that I can confirm my booking.
    /// AC: Booking is confirmed.
    public void CompleteBookingForSeatReservations_SeatReservationsValid_SeatReservationsConfirmed()
    {
        // Arrange
        List<string> seatsRequired = ["A1", "A2", "A3"];
        ISeatReservation seatReservation = _showtime.ProvisionallyReserveSeatsAndReturnReservation(seatsRequired);

        // Act
        IBooking booking = _showtime.CompleteBookingForSeatReservationAndReturnBooking(seatReservation.Id);

        // Assert
        booking.Id.Should().NotBe(Guid.Empty);

        seatReservation.Status.Should().Be(ReservationStatus.Confirmed);
        seatReservation.ReservationTimeoutUtc.Should().Be(null);

        booking.BookingTimeUtc.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Test]
    /// As a Guest,
    /// I want my booking to be rejected if the Seat Reservations are timed out,
    /// So that I'm can reserve seats for a showtime.
    /// AC: Specific exception with message "Seat reservations have expired. Please try again"
    public void ConfirmBookingWithSeatReservations_SeatReservationsExpired_SpecificExceptionThrown()
    {
        // Arrange
        List<string> seatsRequired = ["A1", "A2", "A3"];
        ISeatReservation seatReservation = _showtime.ProvisionallyReserveSeatsAndReturnReservation(seatsRequired);

        seatReservation.SetReservationTimeout(DateTime.UtcNow.AddMinutes(-1));

        // Act & Assert
        ShowtimeException? exception = Assert.Throws<ShowtimeException>(() => _showtime.CompleteBookingForSeatReservationAndReturnBooking(seatReservation.Id));
        exception.Message.Should().Be("Seat reservation timed out");
    }

    [Test]
    /// As a Guest,
    /// I want my booking to be rejected if the seat reservation number is invalid,
    /// So that I can try again or contact the theater.
    /// AC: Specific exception with message "Seat reservation does not exist"
    public void ConfirmBookingWithSeatReservations_SeatReservationDoesNotExist_SpecificExceptionThrown()
    {
        // Arrange
        Guid nonExistentSeatReservationId = Guid.NewGuid();

        // Act & Assert
        ShowtimeException? exception = Assert.Throws<ShowtimeException>(() => _showtime.CompleteBookingForSeatReservationAndReturnBooking(nonExistentSeatReservationId));
        exception.Message.Should().Be("Seat reservation timed out, or invalid");
    }

    [Test]
    /// I want my booking to be rejected if the Seat Reservations have already been confirmed,
    /// So that I know I have already confirmed my booking.
    /// AC: Specific exception with message "Seat reservations have already been booked"
    public void ConfirmBookingWithSeatReservations_SeatReservationsAlreadyConfirmed_SpecificExceptionThrown()
    {
        // Arrange
        List<string> seatsRequired = ["A1", "A2", "A3"];
        ISeatReservation seatReservation = _showtime.ProvisionallyReserveSeatsAndReturnReservation(seatsRequired);

        _showtime.CompleteBookingForSeatReservationAndReturnBooking(seatReservation.Id);

        // Act & Assert
        ShowtimeException? exception = Assert.Throws<ShowtimeException>(() => _showtime.CompleteBookingForSeatReservationAndReturnBooking(seatReservation.Id));
        exception.Message.Should().Be("Seat reservation is already booked");
    }
}