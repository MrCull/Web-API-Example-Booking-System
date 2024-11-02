using Api;
using Api.Services;
using APIDtos;
using Domain.Aggregates.ShowtimeAggregate;
using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;
using Infrastructure.Repository;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Azure.Cosmos;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddAzureCosmosClient("cosmos");


builder.Services.AddScoped<IRepository>(serviceProvider =>
{
    CosmosClient cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
    return new Repository(cosmosClient, "TheaterChainDB", "TheaterChain");
});

const string Expire60Seconds = "Expire60Seconds";

builder.AddRedisOutputCache("cache");
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder =>
        builder.Expire(TimeSpan.FromSeconds(10)));
    options.AddPolicy(Expire60Seconds, builder =>
        builder.Expire(TimeSpan.FromSeconds(60)));
});


builder.Services.AddProblemDetails();

string? JwtKey = builder.Configuration["Jwt:Key"];
string? JwtIssuer = builder.Configuration["Jwt:Issuer"];
string? JwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(JwtKey) || string.IsNullOrEmpty(JwtIssuer) || string.IsNullOrEmpty(JwtAudience))
{
    throw new Exception("Jwt:Key, Jwt:Issuer, and Jwt:Audience must be set in appsettings.json");
}

// local services
builder.Services.AddSingleton<ITheaterChainDtoMapperService, TheaterChainDtoMapperService>();


WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    //ServicePointManager.ServerCertificateValidationCallback +=
    //(sender, cert, chain, sslPolicyErrors) => true;
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseOutputCache();

app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();


# region Movie Management


// Add Theater Chain
app.MapPost("/api/v1/theater-chains", async
       (IRepository theaterChainRepository, CancellationToken cancellationToken, TheaterChainDto theaterChainDto) =>
{
    TheaterChain theaterChain = new(theaterChainDto.Id, theaterChainDto.Name, theaterChainDto.Description);
    await theaterChainRepository.AddAsync(theaterChain, cancellationToken);

    return Results.Created($"/api/v1/theater-chains/{theaterChain.Id}", theaterChain);
});

// Get Movies for chain
app.MapGet("/api/v1/theater-chains/{chainId}/movies", async
    (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, int chainId, CancellationToken cancellationToken)
    =>
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


#endregion Movie Management




#region Theater Management

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
    if (theaterWithIdDto.Id != theaterId) return Results.BadRequest("Theater ID in the URL does not match the ID in the request body.");

    TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
    if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

    // Update the theater with the new details from the theaterDto
    theaterChain.UpdateTheater(theaterWithIdDto.Id, theaterWithIdDto.Name, theaterWithIdDto.Location);

    // Save changes to the repository
    await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

    // Optionally map the updated theater back to a DTO to return in the response
    ITheater? updatedTheater = theaterChain.GetTheaterById(theaterId);
    if (updatedTheater is null) return Results.NotFound($"Theater[{theaterId}] not found in theater chain[{chainId}].");

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
    if (screenDto.Id != screenId) return Results.BadRequest("Screen ID in the URL does not match the ID in the request body.");

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

#endregion Theater Management

#region Theater Showtime Management
// Add Showtime to a Screen
app.MapPost("/api/v1/theater-chains/{chainId}/theaters/{theaterId}/screens/{screenId}/showtimes", async
                      (ITheaterChainDtoMapperService mapperService, IRepository theaterChainRepository, CancellationToken cancellationToken,
                        int chainId, int theaterId, Guid screenId, ShowtimeWithIdDto showtimeDto) =>
{
    TheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
    if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

    ITheater? theater = theaterChain.GetTheaterById(theaterId);
    if (theater is null) return Results.NotFound($"Theater[{theaterId}] not found in theater chain[{chainId}].");

    IShowtime showtime = theater.AddShowtime(showtimeDto.ShowDateTimeUtc, showtimeDto.Price, screenId, showtimeDto.MovieId);

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


#endregion Theater Showtime Management

#region Theater listings
// Get Showtimes for a Theater
app.MapGet("/api/v1/theater-chains/{chainId}/theaters/{theaterId}/screens/{screenId}/showtimes", [OutputCache(PolicyName = Expire60Seconds)] async
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

#endregion Theater listings

#region Reservation Management

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

#endregion Reservation Management

app.Run();
