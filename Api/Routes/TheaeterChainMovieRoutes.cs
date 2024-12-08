using Api.Services;
using APIDtos;
using Domain.Aggregates.TheaterChainAggregate;
using Infrastructure.Repository;

namespace Api.Routes;

public static class TheaterChainMovieRoutes
{
    public static void MapTheaterChainMovieEndpoints(this IEndpointRouteBuilder app)
    {
        // Get Movies for chain
        app.MapGet("/api/v1/theater-chains/{chainId}/movies", async
            (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, int chainId, CancellationToken cancellationToken) =>
        {
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);

            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found");

            List<IMovie> movies = theaterChain.GetMovies();
            IEnumerable<MovieWithIdDto> moviesDto = mapperService.MapMoviesToMoviesWithIdDto(movies);

            return Results.Ok(moviesDto);
        });

        // Get Movie by ID for chain
        app.MapGet("/api/v1/theater-chains/{chainId}/movies/{movieId}", async
            (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, CancellationToken cancellationToken,
            int chainId, int movieId) =>
        {
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found");

            IMovie movie = theaterChain.GetMovieById(movieId);
            if (movie is null)
            {
                return Results.NotFound();
            }

            MovieWithIdDto movieDto = mapperService.MapMovieToMovieDto(movie);
            return Results.Ok(movieDto);
        });

        // Add Movie to chain
        app.MapPost("/api/v1/theater-chains/{chainId}/movies", async
            (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, CancellationToken cancellationToken,
            int chainId, MovieDto movieDto) =>
        {
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found");

            IMovie movieAdded = theaterChain.AddMovie(movieDto.Title, movieDto.Description, movieDto.Genre, movieDto.DurationMins, movieDto.ReleaseDateUtc);

            await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

            MovieWithIdDto addedMovieWithIdDto = mapperService.MapMovieToMovieDto(movieAdded);

            return Results.Created($"/api/v1/theater-chains/{chainId}/movies/{movieAdded.Id}", addedMovieWithIdDto);
        });

        // Update Movie in chain
        app.MapPut("/api/v1/theater-chains/{chainId}/movies/{movieId}", async
            (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, CancellationToken cancellationToken,
            int chainId, int movieId, MovieWithIdDto movieWithIdDto) =>
        {
            if (movieWithIdDto.Id != movieId) return Results.BadRequest("Movie ID in the URL does not match the ID in the request body.");

            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

            // Check if the movie exists in the theater chain
            IMovie existingMovie = theaterChain.GetMovieById(movieWithIdDto.Id);
            if (existingMovie is null) return Results.NotFound($"Movie[{movieWithIdDto?.Id}] not found in the theater chain[{chainId}].");

            // Update the movie with the new details from the movieDto
            theaterChain.UpdateMovie(existingMovie.Id, movieWithIdDto.Title, movieWithIdDto.Description, movieWithIdDto.Genre, movieWithIdDto.DurationMins, movieWithIdDto.ReleaseDateUtc);

            // Save changes to the repository
            await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

            // Optionally map the updated movie back to a DTO to return in the response
            MovieWithIdDto updatedMovieDto = mapperService.MapMovieToMovieDto(existingMovie);

            return Results.Ok(updatedMovieDto);
        });

        // Mark Movie in chain as no longer available
        app.MapPut("/api/v1/theater-chains/{chainId}/movies/{movieId}/no-longer-available", async
            (IRepository theaterChainRepository, ITheaterChainDtoMapperService mapperService,
            int chainId, int movieId, CancellationToken cancellationToken) =>
        {
            // Retrieve the TheaterChain by ID
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

            // Check if the movie exists in the theater chain
            IMovie? existingMovie = theaterChain.GetMovieById(movieId);
            if (existingMovie is null) return Results.NotFound($"Movie[{movieId}] not found in theater chain[{chainId}].");

            // Mark the movie as no longer available
            theaterChain.MarkMovieAsNoLongerAvailable(movieId);

            // Save changes to the repository
            await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

            IMovie movieUpdated = theaterChain.GetMovieById(movieId);
            MovieWithIdDto movieUpdatedDto = mapperService.MapMovieToMovieDto(movieUpdated);

            return Results.Ok(movieUpdatedDto);
        });

        // Mark Movie in chain as available
        app.MapPut("/api/v1/theater-chains/{chainId}/movies/{id}/available", async
               (IRepository theaterChainRepository, ITheaterChainDtoMapperService mapperService,
                  int chainId, int id, CancellationToken cancellationToken) =>
        {
            // Retrieve the TheaterChain by ID
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

            // Check if the movie exists in the theater chain
            IMovie? existingMovie = theaterChain.GetMovieById(id);
            if (existingMovie is null) return Results.NotFound($"Movie[{id}] not found in theater chain[{chainId}].");

            // Mark the movie as available
            theaterChain.MarkMovieAsAvailable(id);

            // Save changes to the repository
            await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

            IMovie movieUpdated = theaterChain.GetMovieById(id);
            MovieWithIdDto movieUpdatedDto = mapperService.MapMovieToMovieDto(movieUpdated);

            return Results.Ok(movieUpdatedDto);
        });
    }
}
