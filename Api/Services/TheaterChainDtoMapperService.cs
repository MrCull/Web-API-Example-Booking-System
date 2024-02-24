using Api.Dtos;
using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;

namespace Api.Services
{
    public class TheaterChainDtoMapperService : ITheaterChainDtoMapperService
    {
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

        public TheaterWithIdDto MapTheaterToTheaterWithIdDto(ITheater theater)
            => new(theater.Id, theater.Name, theater.Location);

        public IEnumerable<TheaterWithIdDto> MapTheatersToTheatersWithIdDto(List<ITheater> theaters)
            => theaters.Select(MapTheaterToTheaterWithIdDto);
    }
}
