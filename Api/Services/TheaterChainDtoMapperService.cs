using APIDtos;
using Domain.Aggregates.ShowtimeAggregate;
using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;

namespace Api.Services
{
    public class TheaterChainDtoMapperService : ITheaterChainDtoMapperService
    {
        #region Movies

        public MovieWithIdDto MapMovieToMovieDto(IMovie movie)
        {
            MovieStatus movieStatus = movie.TheaterChainMovieStatus switch
            {
                TheaterChainMovieStatus.Available => MovieStatus.Available,
                TheaterChainMovieStatus.NoLongerAvailable => MovieStatus.NoLongerAvailable,
                _ => throw new ArgumentOutOfRangeException()
            };

            return new MovieWithIdDto(movie.Id, movie.Title, movie.Description, movie.DurationMins, movie.Genre, movie.ReleaseDateUtc, movieStatus);
        }

        public IEnumerable<MovieWithIdDto> MapMoviesToMoviesWithIdDto(List<IMovie> movies)
            => movies.Select(MapMovieToMovieDto);

        #endregion Movies

        #region Theaters

        public TheaterWithIdDto MapTheaterToTheaterWithIdDto(ITheater theater)
        {
            List<ScreenWithIdDto> x = MapScreensToScreensWithIdDto(theater.GetScreens());
            return new(theater.Id, theater.Name, theater.Location, x);
        }

        public IEnumerable<TheaterWithIdDto> MapTheatersToTheatersWithIdDto(List<ITheater> theaters)
            => theaters.Select(MapTheaterToTheaterWithIdDto);

        #endregion Theaters

        #region Screens

        public ScreenWithIdDto MapScreenToScreenWithIdDto(IScreen screen)
        {
            List<SeatWithIdDto> seats = MapSeatsToSeatsWithIdDto(screen.GetSeats());
            return new(screen.Id, screen.ScreenNumber, screen.IsEnabled, seats);
        }

        public List<ScreenWithIdDto> MapScreensToScreensWithIdDto(List<IScreen> screens)
            => screens.Select(MapScreenToScreenWithIdDto).ToList();

        #endregion Screens

        #region Seats

        public SeatWithIdDto MapSeatsToSeatsWithIdDto(ISeat seat)
        {
            return new(seat.Id, seat.SeatNumber);
        }

        public List<SeatWithIdDto> MapSeatsToSeatsWithIdDto(List<ISeat> seats)
            => seats.Select(MapSeatsToSeatsWithIdDto).ToList();

        public List<ISeat> MapSeatsWithIdDtoToSeats(List<SeatWithIdDto> seats)
            => seats.Select(s => (ISeat)new Seat(s.SeatNumber, s.Id)).ToList();

        #endregion Seats

        #region Showtimes
        public ShowtimeWithIdDto MapShowtimeToShowtimeWithIdDto(IShowtime showtime)
        {
            return new(showtime.Id, showtime.MovieId, showtime.ScreenId, showtime.ShowDateTimeUtc, showtime.Price);
        }

        #endregion Showtimes

        #region SeatReservations

        public SeatReservationWithIdDto MapSeatReservationToSeatReservationWithIdDto(ISeatReservation reservation)
        {
            SeatReservationWithIdDto.SeatReservationStatus reservationStatus = reservation.Status switch
            {
                ReservationStatus.Reserved => SeatReservationWithIdDto.SeatReservationStatus.Provisional,
                ReservationStatus.Confirmed => SeatReservationWithIdDto.SeatReservationStatus.Confirmed,
                _ => throw new ArgumentOutOfRangeException()
            };

            return new(reservation.Id, reservation.Seats.Select(s => s.SeatNumber).ToList(), reservation.ShowtimeId, reservation.ReservationTimeoutUtc, reservationStatus);
        }

        public BookingDto MapBookingToBookingDto(IBooking booking)
        {
            return new(booking.Id, booking.BookingTimeUtc, MapSeatReservationToSeatReservationWithIdDto(booking.SeatReservation));
        }

        #endregion SeatReservations


    }
}
