using Api.Dtos;
using Domain.Aggregates.TheaterChainAggregate;

namespace Api.Services;

public interface ITheaterChainDtoMapperService
{
    MovieDto MapMovieToMovideDto(Movie movie);
    IEnumerable<MovieDto> MapMovieToMovideDto(List<Movie> movies);
}