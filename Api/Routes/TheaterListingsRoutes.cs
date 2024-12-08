using Domain.Aggregates.ShowtimeAggregate;
using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;
using Infrastructure.Repository;
using Microsoft.AspNetCore.OutputCaching;

namespace Api.Routes;

public static class TheaterListingsRoutes
{
    public static void MapTheaterListingsEndpoints(this IEndpointRouteBuilder app)
    {
        // Get Showtimes for a Theater
        app.MapGet("/api/v1/theater-chains/{chainId}/theaters/{theaterId}/screens/{screenId}/showtimes", [OutputCache(PolicyName = "Expire60Seconds")] async
            (IRepository theaterChainRepository, CancellationToken cancellationToken,
             int chainId, int theaterId) =>
        {
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

            ITheater? theater = theaterChain.GetTheaterById(theaterId);
            if (theater is null) return Results.NotFound($"Theater[{theaterId}] not found in theater chain[{chainId}].");

            IEnumerable<IShowtime> showtimes = theater.GetActiveShowtimes();

            return Results.Ok(showtimes);
        });
    }
}
