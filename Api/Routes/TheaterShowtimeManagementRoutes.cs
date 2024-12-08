using Api.Services;
using APIDtos;
using Domain.Aggregates.ShowtimeAggregate;
using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;
using Infrastructure.Repository;

namespace Api.Routes;

public static class TheaterShowtimeManagementRoutes
{
    public static void MapTheaterShowtimeManagementEndpoints(this IEndpointRouteBuilder app)
    {
        // Add Showtime to a Screen
        app.MapPost("/api/v1/theater-chains/{chainId}/theaters/{theaterId}/screens/{screenId}/showtimes", async
            (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, CancellationToken cancellationToken,
             int chainId, int theaterId, Guid screenId, ShowtimeWithIdDto showtimeDto) =>
        {
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

            ITheater? theater = theaterChain.GetTheaterById(theaterId);
            if (theater is null) return Results.NotFound($"Theater[{theaterId}] not found in theater chain[{chainId}].");

            IShowtime showtime = theater.AddShowtime(
                showtimeDto.ShowDateTimeUtc,
                showtimeDto.Price,
                screenId,
                showtimeDto.MovieId
            );

            await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

            ShowtimeWithIdDto showtimeWithIdDto = mapperService.MapShowtimeToShowtimeWithIdDto(showtime);

            return Results.Created($"/api/v1/theater-chains/{chainId}/theaters/{theaterId}/screens/{screenId}/showtimes/{showtime.Id}", showtimeWithIdDto);
        });

        // Remove Showtime from a Screen
        app.MapDelete("/api/v1/theater-chains/{chainId}/theaters/{theaterId}/screens/{screenId}/showtimes/{showtimeId}", async
            (IRepository theaterChainRepository, CancellationToken cancellationToken,
             int chainId, int theaterId, Guid screenId, int showtimeId) =>
        {
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

            ITheater? theater = theaterChain.GetTheaterById(theaterId);
            if (theater is null) return Results.NotFound($"Theater[{theaterId}] not found in theater chain[{chainId}].");

            theater.RemoveShowtime(showtimeId);

            await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

            return Results.NoContent();
        });
    }
}
