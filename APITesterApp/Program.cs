using APIDtos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

HttpLoggingHandler handler = new(new HttpClientHandler());
HttpClient client = new(handler)
{
    BaseAddress = new Uri("https://localhost:7091/"), // API base address (usually form a configuration)
    Timeout = TimeSpan.FromHours(1)
};

client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwtToken());

try
{
    #region Test: Add Theater Chain
    TheaterChainDto Odean = new(1, "Odean", "Odeon Cinemas Group");
    HttpResponseMessage createChainResponse = await client.PostAsJsonAsync("/api/v1/theater-chains", Odean);
    if (!createChainResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to create theater chain: {await createChainResponse.Content.ReadAsStringAsync()}");
        return;
    }

    Console.WriteLine($"Theater chain created successfully [{Odean}].");

    // Test: Get Theaters for a Chain
    HttpResponseMessage getTheatersResponse = await client.GetAsync($"/api/v1/theater-chains/{Odean.Id}/theaters");
    if (!getTheatersResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to get theaters: {await getTheatersResponse.Content.ReadAsStringAsync()}");
        return;
    }
    Console.WriteLine("Successfully retrieved theaters list.");

    #endregion Test: Add Theater Chain



    #region Add a new movie to the chain
    MovieDto newMovie = new("The Matrix", "A computer hacker learns from mysterious rebels about the true nature of his reality and his.", 136, "Action, Sci-Fi", new DateTime(1999, 5, 4));
    HttpResponseMessage addMovieResponse = await client.PostAsJsonAsync($"/api/v1/theater-chains/{Odean.Id}/movies", newMovie);
    if (!addMovieResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to add movie: {await addMovieResponse.Content.ReadAsStringAsync()}");
        return;
    }

    MovieWithIdDto? addedMovie = await addMovieResponse.Content.ReadFromJsonAsync<MovieWithIdDto>();
    if (addedMovie is null)
    {
        Console.WriteLine("Failed to deserialize movie response.");
        return;
    }

    Console.WriteLine("Movie added successfully.");

    // Update a movie in the chain
    MovieWithIdDto updateMovie = new(addedMovie.Id, addedMovie.Title, addedMovie.Description, addedMovie.DurationMins, addedMovie.Genre, new DateTime(1999, 3, 4), addedMovie.MovieStatus);
    HttpResponseMessage updateMovieResponse = await client.PutAsJsonAsync($"/api/v1/theater-chains/{Odean.Id}/movies/{addedMovie.Id}", updateMovie);
    if (!updateMovieResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to update movie: {await updateMovieResponse.Content.ReadAsStringAsync()}");
        return;
    }

    MovieWithIdDto? theMatrix = await updateMovieResponse.Content.ReadFromJsonAsync<MovieWithIdDto>();

    if (theMatrix is null)
    {
        Console.WriteLine("Failed to deserialize movie response.");
        return;
    }

    Console.WriteLine("Movie updated successfully.");

    #endregion Add a new movie to the chain



    #region Add a theater to the chain with screen and seats

    // Create a new theater in the chain
    TheaterDto newTheater = new("Loughborough Max", "Market Street");
    HttpResponseMessage createTheaterResponse = await client.PostAsJsonAsync($"/api/v1/theater-chains/{Odean.Id}/theaters", newTheater);
    if (!createTheaterResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to create theater: {await createTheaterResponse.Content.ReadAsStringAsync()}");
        return;
    }
    TheaterWithIdDto? theaterLoughborough = await createTheaterResponse.Content.ReadFromJsonAsync<TheaterWithIdDto>();

    if (theaterLoughborough is null)
    {
        Console.WriteLine("Failed to deserialize theater response.");
        return;
    }

    Console.WriteLine($"Theater '{theaterLoughborough.Name}' created successfully with ID: {theaterLoughborough.Id}");

    // Add a screen with seats to the newly created theater
    ScreenDto newScreen = new("Screen 1", true)
    {
        Seats =
        [
            new SeatDto("A1"),
            new SeatDto("A2")
        ]
    };

    HttpResponseMessage addScreenResponse = await client.PostAsJsonAsync($"/api/v1/theater-chains/{Odean.Id}/theaters/{theaterLoughborough.Id}/screens", newScreen);
    if (!addScreenResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to add screen: {await addScreenResponse.Content.ReadAsStringAsync()}");
        return;
    }
    ScreenWithIdDto? screen1 = await addScreenResponse.Content.ReadFromJsonAsync<ScreenWithIdDto>();

    if (screen1 is null)
    {
        Console.WriteLine("Failed to deserialize screen response.");
        return;
    }

    Console.WriteLine($"Screen {screen1.ScreenNumber} added successfully with ID: {screen1.Id}");

    #endregion Add a theater to the chain with screen and seats



    #region Add a showtime to a theater in the chain

    ShowtimeDto newShowtime = new(theMatrix.Id, screen1.Id, DateTime.UtcNow.AddDays(1), 12.50M);

    HttpResponseMessage createShowtimeResponse = await client.PostAsJsonAsync($"/api/v1/theater-chains/{Odean.Id}/theaters/{theaterLoughborough.Id}/screens/{screen1.Id}/showtimes", newShowtime);
    if (!createShowtimeResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to create showtime: {await createShowtimeResponse.Content.ReadAsStringAsync()}");
        return;
    }
    ShowtimeWithIdDto? showtimeAddedDto = await createShowtimeResponse.Content.ReadFromJsonAsync<ShowtimeWithIdDto>();

    if (showtimeAddedDto is null)
    {
        Console.WriteLine("Failed to deserialize showtime response.");
        return;
    }

    Console.WriteLine($"Showtime created successfully for movie ID: {showtimeAddedDto.MovieId} at {showtimeAddedDto.ShowDateTimeUtc}, Showtime ID: {showtimeAddedDto.Id}");

    #endregion

    #region make reservation for a showtime and confirm booking

    SeatReservationDto reservationDto = new(["A1", "A2"], showtimeAddedDto.Id);

    HttpResponseMessage makeReservationResponse = await client.PostAsJsonAsync($"/api/v1/theater-chains/{Odean.Id}/theaters/{theaterLoughborough.Id}/screens/{screen1.Id}/showtimes/{showtimeAddedDto.Id}/reservations", reservationDto);
    if (!makeReservationResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to make a reservation: {await makeReservationResponse.Content.ReadAsStringAsync()}");
        return;
    }
    SeatReservationWithIdDto? pendingReservation = await makeReservationResponse.Content.ReadFromJsonAsync<SeatReservationWithIdDto>();

    if (pendingReservation is null)
    {
        Console.WriteLine("Failed to deserialize reservation response.");
        return;
    }

    Console.WriteLine($"Reservation made successfully with ID: {pendingReservation.Id}");

    // Confirm the reservation to complete the booking
    HttpResponseMessage confirmBookingResponse = await client.PutAsJsonAsync($"/api/v1/theater-chains/{Odean.Id}/theaters/{theaterLoughborough.Id}/screens/{screen1.Id}/showtimes/{pendingReservation.ShowtimeId}/reservations/{pendingReservation.Id}/confirm", pendingReservation);

    if (!confirmBookingResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to confirm booking: {await confirmBookingResponse.Content.ReadAsStringAsync()}");
        return;
    }

    BookingDto? booking = await confirmBookingResponse.Content.ReadFromJsonAsync<BookingDto>();

    if (booking is null)
    {
        Console.WriteLine("Failed to deserialize booking response.");
        return;
    }

    Console.WriteLine($"Booking confirmed successfully for reservation ID: {booking.Id} with details [{booking}]");

    #endregion make reservation for a showtime and confirm booking


}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}



static string GenerateJwtToken()
{
    // This is just a local run test app, so ok to hardcode a dummy securityKey
    SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes("0F92A643-AD74-429C-8302-E52BC8D4BD6E"));
    SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

    Claim[] claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, "MovieBookingUser"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    JwtSecurityToken token = new(
        issuer: "MovieBookingApiTest",
        audience: "Movie Booking Test",
        claims: claims,
        expires: DateTime.Now.AddMinutes(60),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}