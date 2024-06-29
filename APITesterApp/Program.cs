using System.Net.Http.Json;

HttpClient client = new()
{
    BaseAddress = new Uri("https://localhost:5001/") // Set this to your API base address
};

try
{
    // Test: Add Theater Chain
    var newTheaterChain = new TheaterChainDto { Id = 1, Name = "Best Chain", Description = "Top notch theaters" };
    var createChainResponse = await client.PostAsJsonAsync("/api/v1/theater-chains", newTheaterChain);
    if (!createChainResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to create theater chain: {await createChainResponse.Content.ReadAsStringAsync()}");
        return;
    }
    Console.WriteLine("Theater chain created successfully.");

    // Test: Get Theaters for a Chain
    var getTheatersResponse = await client.GetAsync($"/api/v1/theater-chains/{newTheaterChain.Id}/theaters");
    if (!getTheatersResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to get theaters: {await getTheatersResponse.Content.ReadAsStringAsync()}");
        return;
    }
    Console.WriteLine("Successfully retrieved theaters list.");

    // Add a new movie to the chain
    var newMovie = new MovieDto { Title = "Blockbuster", Description = "Action-packed", Genre = "Action", DurationMins = 120, ReleaseDateUtc = DateTime.UtcNow };
    var addMovieResponse = await client.PostAsJsonAsync($"/api/v1/theater-chains/{newTheaterChain.Id}/movies", newMovie);
    if (!addMovieResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to add movie: {await addMovieResponse.Content.ReadAsStringAsync()}");
        return;
    }
    Console.WriteLine("Movie added successfully.");

    // Update a movie in the chain
    int movieId = 1; // Assume movieId is known or fetched from previous response
    var updateMovie = new MovieWithIdDto { Id = movieId, Title = "Blockbuster Updated", Description = "More explosions!", Genre = "Action", DurationMins = 130, ReleaseDateUtc = DateTime.UtcNow };
    var updateMovieResponse = await client.PutAsJsonAsync($"/api/v1/theater-chains/{newTheaterChain.Id}/movies/{movieId}", updateMovie);
    if (!updateMovieResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to update movie: {await updateMovieResponse.Content.ReadAsStringAsync()}");
        return;
    }
    Console.WriteLine("Movie updated successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}