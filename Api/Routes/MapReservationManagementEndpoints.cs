using Api.Services;
using APIDtos;
using Domain.Aggregates.ShowtimeAggregate;
using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;
using Infrastructure.Repository;

namespace Api.Routes;

public static class ReservationManagementRoutes
{
    public static void MapReservationManagementEndpoints(this IEndpointRouteBuilder app)
    {
        // Make a reservation for a showtime
        app.MapPost("/api/v1/theater-chains/{chainId}/theaters/{theaterId}/screens/{screenId}/showtimes/{showtimeId}/reservations", async
            (IRepository theaterChainRepository, CancellationToken cancellationToken, ITheaterChainDtoMapperService mapperService,
             int chainId, int theaterId, Guid screenId, int showtimeId, SeatReservationDto reservationDto) =>
        {
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

            ITheater? theater = theaterChain.GetTheaterById(theaterId);
            if (theater is null) return Results.NotFound($"Theater[{theaterId}] not found in theater chain[{chainId}].");

            IShowtime? showtime = theater.GetActiveShowtimeById(showtimeId);
            if (showtime is null) return Results.NotFound($"Showtime[{showtimeId}] not found in theater[{theaterId}].");

            ISeatReservation reservation = showtime.ProvisionallyReserveSeatsAndReturnReservation(reservationDto.Seats);

            SeatReservationWithIdDto reservationWithIdDto = mapperService.MapSeatReservationToSeatReservationWithIdDto(reservation);

            await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

            return Results.Created($"/api/v1/theater-chains/{chainId}/theaters/{theaterId}/screens/{screenId}/showtimes/{showtimeId}/reservations/{reservation.Id}", reservationWithIdDto);
        });

        // Confirm a reservation
        app.MapPut("/api/v1/theater-chains/{chainId}/theaters/{theaterId}/screens/{screenId}/showtimes/{showtimeId}/reservations/{reservationId}/confirm", async
            (IRepository theaterChainRepository, CancellationToken cancellationToken, ITheaterChainDtoMapperService mapperService,
             int chainId, int theaterId, Guid screenId, int showtimeId, Guid reservationId) =>
        {
            TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
            if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

            ITheater? theater = theaterChain.GetTheaterById(theaterId);
            if (theater is null) return Results.NotFound($"Theater[{theaterId}] not found in theater chain[{chainId}].");

            IShowtime? showtime = theater.GetActiveShowtimeById(showtimeId);
            if (showtime is null) return Results.NotFound($"Showtime[{showtimeId}] not found in theater[{theaterId}].");

            IBooking booking = showtime.CompleteBookingForSeatReservationAndReturnBooking(reservationId);

            await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

            BookingDto bookingDto = mapperService.MapBookingToBookingDto(booking);

            return Results.Ok(bookingDto);
        });
    }
}
