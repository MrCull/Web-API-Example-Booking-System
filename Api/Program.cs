using Api;
using Api.Dtos;
using Api.Services;
using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    return new CosmosClient(
        accountEndpoint: "https://localhost:8081/",
        // Telow is the common key that is used for all local Cosmos DB Emulator instances.
        // This is hardcoded because it is not a secret and is only used for local development.
        // https://learn.microsoft.com/en-us/azure/cosmos-db/emulator
        authKeyOrResourceToken: "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
    );
});

builder.Services.AddScoped<IRepository<ITheaterChain>>(serviceProvider =>
{
    CosmosClient cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
    return new Repository<ITheaterChain>(cosmosClient, "TheaterChainDB", "TheaterChain");
});

string? JwtKey = builder.Configuration["Jwt:Key"];
string? JwtIssuer = builder.Configuration["Jwt:Issuer"];
string? JwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(JwtKey) || string.IsNullOrEmpty(JwtIssuer) || string.IsNullOrEmpty(JwtAudience))
{
    throw new Exception("Jwt:Key, Jwt:Issuer, and Jwt:Audience must be set in appsettings.json");
}


// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JwtAudience,
            ValidAudience = JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey))
        };
    });


builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireClaim("Role", "Admin"))
    .AddDefaultPolicy("RequireAuthenticatedUser", policy => policy.RequireAuthenticatedUser());

//.AddDefaultPolicy
//  .AddDefaultPolicy(policy =>
//  {
//      policy.RequireAuthenticatedUser();
//  });



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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

# region TheaterChain

# region Movie Management


// Add Theater Chain
app.MapPost("/api/v1/theater-chains", async
       (IRepository<ITheaterChain> theaterChainRepository, CancellationToken cancellationToken) =>
{
    ITheaterChain theaterChain = new TheaterChain(8, "Odean", "Oddy");
    string json = JsonSerializer.Serialize(theaterChain);
    await theaterChainRepository.AddAsync(theaterChain, cancellationToken);

    return Results.Created($"/api/v1/theater-chains/{theaterChain.Id}", theaterChain);
});

// Get Movies for chain
app.MapGet("/api/v1/theater-chains/{chainId}/movies", async
    (ITheaterChainDtoMapperService mapperService, IRepository<ITheaterChain> theaterChainRepository, int chainId, CancellationToken cancellationToken)
    =>
{
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);

    if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found");

    List<IMovie> movies = theaterChain.GetMovies();
    IEnumerable<MovieWithIdDto> moviesDto = mapperService.MapMoviesToMoviesWithIdDto(movies);

    return Results.Ok(moviesDto);
});

// Get Movie by ID for chain
app.MapGet("/api/v1/theater-chains/{chainId}/movies/{movieId}", async
    (ITheaterChainDtoMapperService mapperService, IRepository<ITheaterChain> theaterChainRepository, CancellationToken cancellationToken,
    int chainId, int movieId) =>
{
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
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
    (ITheaterChainDtoMapperService mapperService, IRepository<ITheaterChain> theaterChainRepository, CancellationToken cancellationToken,
    int chainId, MovieDto movieDto) =>
{
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
    if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found");

    IMovie movieAdded = theaterChain.AddMovie(movieDto.Title, movieDto.Description, movieDto.Genre, movieDto.DurationMins, movieDto.ReleaseDateUtc);

    await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

    MovieWithIdDto addedMovieWithIdDto = mapperService.MapMovieToMovieDto(movieAdded);

    return Results.Created($"/api/v1/theater-chains/{chainId}/movies/{movieAdded.Id}", addedMovieWithIdDto);
});

// Update Movie in chain
app.MapPut("/api/v1/theater-chains/{chainId}/movies/{id}", async
    (ITheaterChainDtoMapperService mapperService, IRepository<ITheaterChain> theaterChainRepository, CancellationToken cancellationToken,
    int chainId, int movieId, MovieWithIdDto movieWithIdDto) =>
{
    if (movieWithIdDto.Id != movieId) return Results.BadRequest("Movie ID in the URL does not match the ID in the request body.");

    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
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
    (IRepository<ITheaterChain> theaterChainRepository, ITheaterChainDtoMapperService mapperService,
    int chainId, int movieId, CancellationToken cancellationToken) =>
{
    // Retrieve the TheaterChain by ID
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
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
       (IRepository<ITheaterChain> theaterChainRepository, ITheaterChainDtoMapperService mapperService,
          int chainId, int id, CancellationToken cancellationToken) =>
{
    // Retrieve the TheaterChain by ID
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
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

// Get Theaters for chain
app.MapGet("/api/v1/theater-chains/{chainId}/theaters", async
       (ITheaterChainDtoMapperService mapperService, IRepository<ITheaterChain> theaterChainRepository, int chainId, CancellationToken cancellationToken) =>
{
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
    if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found");

    IEnumerable<TheaterWithIdDto> theatersDto = mapperService.MapTheatersToTheatersWithIdDto(theaterChain.GetTheaters());
    return Results.Ok(theatersDto);
});

// Get Theater by ID for chain
app.MapGet("/api/v1/theater-chains/{chainId}/theaters/{theaterId}", async
       (ITheaterChainDtoMapperService mapperService, IRepository<ITheaterChain> theaterChainRepository, CancellationToken cancellationToken,
          int chainId, int theaterId) =>
{
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
    if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found");

    ITheater? theater = theaterChain.GetTheaterById(theaterId);
    if (theater is null) return Results.NotFound();

    TheaterWithIdDto theaterDto = mapperService.MapTheaterToTheaterWithIdDto(theater);
    return Results.Ok(theaterDto);
});

// Add Theater to chain
app.MapPost("/api/v1/theater-chains/{chainId}/theaters", async
          (ITheaterChainDtoMapperService mapperService, IRepository<ITheaterChain> theaterChainRepository, CancellationToken cancellationToken,
                   int chainId, TheaterDto theaterDto) =>
{
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
    if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found");

    ITheater theaterAdded = theaterChain.AddTheater(theaterDto.Name, theaterDto.Location);
    await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

    TheaterWithIdDto theaterAddedDto = mapperService.MapTheaterToTheaterWithIdDto(theaterAdded);

    return Results.Created($"/api/v1/theater-chains/{chainId}/theaters/{theaterAdded.Id}", theaterAddedDto);
});

// Update Theater in chain
app.MapPut("/api/v1/theater-chains/{chainId}/theaters/{theaterId}", async
             (ITheaterChainDtoMapperService mapperService, IRepository<ITheaterChain> theaterChainRepository, CancellationToken cancellationToken,
                               int chainId, int theaterId, TheaterWithIdDto theaterWithIdDto) =>
{
    if (theaterWithIdDto.Id != theaterId) return Results.BadRequest("Theater ID in the URL does not match the ID in the request body.");

    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
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

#endregion TheaterChain


app.Run();
