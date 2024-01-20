using Api.Dtos;
using Domain.Aggregates.TheaterChainAggregate;

namespace Api.Services;

public interface ITheaterChainDtoMapperService
{
    MovieDto MapMovieToMovieDto(Movie movie);
    IEnumerable<MovieDto> MapMoviesToMoviesDto(List<Movie> movies);
}