using Api.Services;
using APIDtos;
using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;
using Infrastructure.Repository;
using Microsoft.AspNetCore.OutputCaching;

namespace Api.Routes;

public static class TheaterManagementRoutes
{
    public static void MapTheaterManagementEndpoints(this IEndpointRouteBuilder app)
    {
        // Get Theaters for chain
        app.MapGet("/api/v1/theater-chains/{chainId}/theaters", async
            (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, int chainId, CancellationToken cancellationToken) =>
        {
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found");

            IEnumerable<TheaterWithIdDto> theatersDto = mapperService.MapTheatersToTheatersWithIdDto(theaterChain.GetTheaters());
            return Results.Ok(theatersDto);
        });

        // Get Theater by ID for chain
        app.MapGet("/api/v1/theater-chains/{chainId}/theaters/{theaterId}", [OutputCache] async
            (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, CancellationToken cancellationToken,
             int chainId, int theaterId) =>
        {
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found");

            ITheater? theater = theaterChain.GetTheaterById(theaterId);
            if (theater is null) return Results.NotFound();

            TheaterWithIdDto theaterDto = mapperService.MapTheaterToTheaterWithIdDto(theater);
            return Results.Ok(theaterDto);
        });

        // Add Theater to chain
        app.MapPost("/api/v1/theater-chains/{chainId}/theaters", async
            (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, CancellationToken cancellationToken,
             int chainId, TheaterDto theaterDto) =>
        {
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found");

            ITheater theaterAdded = theaterChain.AddTheater(theaterDto.Name, theaterDto.Location);
            await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

            TheaterWithIdDto theaterAddedDto = mapperService.MapTheaterToTheaterWithIdDto(theaterAdded);

            return Results.Created($"/api/v1/theater-chains/{chainId}/theaters/{theaterAdded.Id}", theaterAddedDto);
        });

        // Update Theater in chain
        app.MapPut("/api/v1/theater-chains/{chainId}/theaters/{theaterId}", async
            (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, CancellationToken cancellationToken,
             int chainId, int theaterId, TheaterWithIdDto theaterWithIdDto) =>
        {
            if (theaterWithIdDto.Id != theaterId)
                return Results.BadRequest("Theater ID in the URL does not match the ID in the request body.");

            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

            // Update the theater with the new details from the theaterDto
            theaterChain.UpdateTheater(theaterWithIdDto.Id, theaterWithIdDto.Name, theaterWithIdDto.Location);

            // Save changes to the repository
            await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

            // Optionally map the updated theater back to a DTO to return in the response
            ITheater? updatedTheater = theaterChain.GetTheaterById(theaterId);
            if (updatedTheater is null)
                return Results.NotFound($"Theater[{theaterId}] not found in theater chain[{chainId}].");

            TheaterWithIdDto updatedTheaterDto = mapperService.MapTheaterToTheaterWithIdDto(updatedTheater);

            return Results.Ok(updatedTheaterDto);
        });

        // Add a Screen to a Theater
        app.MapPost("/api/v1/theater-chains/{chainId}/theaters/{theaterId}/screens", async
            (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, CancellationToken cancellationToken,
             int chainId, int theaterId, ScreenDto screenDto) =>
        {
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

            ITheater? theater = theaterChain.GetTheaterById(theaterId);
            if (theater is null) return Results.NotFound($"Theater[{theaterId}] not found in theater chain[{chainId}].");

            List<string> seats = screenDto.Seats.Select(s => s.SeatNumber).ToList();

            IScreen screenAdded = theater.AddScreen(screenDto.ScreenNumber, seats);
            await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

            ScreenWithIdDto screenAddedDto = mapperService.MapScreenToScreenWithIdDto(screenAdded);

            return Results.Created($"/api/v1/theater-chains/{chainId}/theaters/{theaterId}/screens/{screenAdded.Id}", screenAddedDto);
        });

        // Update a Screen in a Theater
        app.MapPut("/api/v1/theater-chains/{chainId}/theaters/{theaterId}/screens/{screenId}", async
            (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, CancellationToken cancellationToken,
             int chainId, int theaterId, Guid screenId, ScreenWithIdDto screenDto) =>
        {
            if (screenDto.Id != screenId)
                return Results.BadRequest("Screen ID in the URL does not match the ID in the request body.");

            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

            ITheater? theater = theaterChain.GetTheaterById(theaterId);
            if (theater is null) return Results.NotFound($"Theater[{theaterId}] not found in theater chain[{chainId}].");

            List<string> seats = screenDto.Seats.Select(s => s.SeatNumber).ToList();

            IScreen updatedScreen = theater.UpdateScreen(screenDto.Id, screenDto.ScreenNumber, seats);
            await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

            ScreenWithIdDto updatedScreenDto = mapperService.MapScreenToScreenWithIdDto(updatedScreen);

            return Results.Ok(updatedScreenDto);
        });
    }
}