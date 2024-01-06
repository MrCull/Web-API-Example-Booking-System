using Api.Dtos;
using CinemaBooking.Model;

namespace Api.Services;

public interface ITheaterChainDtoMapperService
{
    MovieDto MapMovieToMovideDto(Movie movie);
    IEnumerable<MovieDto> MapMovieToMovideDto(List<Movie> movies);
}