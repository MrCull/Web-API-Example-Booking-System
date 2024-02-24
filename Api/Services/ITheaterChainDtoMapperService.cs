using Api.Dtos;
using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;

namespace Api.Services;

public interface ITheaterChainDtoMapperService
{
    MovieWithIdDto MapMovieToMovieDto(IMovie movie);
    IEnumerable<MovieWithIdDto> MapMoviesToMoviesWithIdDto(List<IMovie> movies);

    TheaterWithIdDto MapTheaterToTheaterWithIdDto(ITheater theater);
    IEnumerable<TheaterWithIdDto> MapTheatersToTheatersWithIdDto(List<ITheater> theaters);
}