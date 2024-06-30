using APIDtos;
using Domain.Aggregates.ShowtimeAggregate;
using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;

namespace Api.Services;

public interface ITheaterChainDtoMapperService
{
    MovieWithIdDto MapMovieToMovieDto(IMovie movie);
    IEnumerable<MovieWithIdDto> MapMoviesToMoviesWithIdDto(List<IMovie> movies);

    TheaterWithIdDto MapTheaterToTheaterWithIdDto(ITheater theater);
    IEnumerable<TheaterWithIdDto> MapTheatersToTheatersWithIdDto(List<ITheater> theaters);
    ScreenWithIdDto MapScreenToScreenWithIdDto(IScreen screen);
    ShowtimeWithIdDto MapShowtimeToShowtimeWithIdDto(IShowtime showtime);
    SeatReservationWithIdDto MapSeatReservationToSeatReservationWithIdDto(ISeatReservation reservation);
}