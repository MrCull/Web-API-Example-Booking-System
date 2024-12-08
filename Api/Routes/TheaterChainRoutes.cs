using APIDtos;
using Domain.Aggregates.TheaterChainAggregate;
using Infrastructure.Repository;

namespace Api.Routes;

public static class TheaterChainRoutes
{
    public static void MapTheaterChainEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/theater-chains", async
            (IRepository theaterChainRepository, CancellationToken cancellationToken, TheaterChainDto theaterChainDto) =>
        {
            TheaterChain theaterChain = new(theaterChainDto.Id, theaterChainDto.Name, theaterChainDto.Description);
            await theaterChainRepository.AddAsync(theaterChain, cancellationToken);
            return Results.Created($"/api/v1/theater-chains/{theaterChain.Id}", theaterChain);
        }).WithTags("Theater Chains");
    }
}

