using Api.Dtos;
using Domain.Aggregates.TheaterChainAggregate;

namespace Api.Services
{
    public class TheaterChainDtoMapperService : ITheaterChainDtoMapperService
    {
        public MovieDto MapMovieToMovieDto(IMovie movie)
        {
            MovieStatus movieStatus = movie.TheaterChainMovieStatus switch
            {
                TheaterChainMovieStatus.Available => MovieStatus.Available,
                TheaterChainMovieStatus.NoLongerAvailable => MovieStatus.NoLongerAvailable,
                _ => throw new ArgumentOutOfRangeException()
            };

            return new MovieDto(movie.Id, movie.Title, movie.Description, movie.DurationMins, movie.Genre, movie.ReleaseDateUtc, movieStatus);
        }

        public IEnumerable<MovieDto> MapMoviesToMoviesDto(List<IMovie> movies)
            => movies.Select(MapMovieToMovieDto);
    }
}
