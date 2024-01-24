using Api.Dtos;
using Domain.Aggregates.TheaterChainAggregate;

namespace Api.Services;

public interface ITheaterChainDtoMapperService
{
    MovieDto MapMovieToMovieDto(IMovie movie);
    IEnumerable<MovieDto> MapMoviesToMoviesDto(List<IMovie> movies);
}