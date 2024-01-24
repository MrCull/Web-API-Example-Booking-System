using Api;
using Api.Dtos;
using Api.Services;
using Domain.Aggregates.TheaterChainAggregate;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    return new CosmosClient(
        accountEndpoint: "https://localhost:8081/",
        // below is the common key that is used for all local Cosmos DB Emulator instances
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

app.MapPost("/api/v1/theater-chains/{chainId}/movies", async
    (ITheaterChainDtoMapperService mapperService, IRepository<ITheaterChain> theaterChainRepository, CancellationToken cancellationToken,
    int chainId, MovieWithoutIdDto movieWithoutIdDto) =>
{
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
    if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found");

    IMovie movieAdded = theaterChain.AddMovie(movieWithoutIdDto.Title, movieWithoutIdDto.Description, movieWithoutIdDto.Genre, movieWithoutIdDto.DurationMins, movieWithoutIdDto.ReleaseDateUtc);

    await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

    MovieDto movieAddedDto = mapperService.MapMovieToMovieDto(movieAdded);

    return Results.Created($"/api/v1/theater-chains/{chainId}/movies/{movieAdded.Id}", movieAddedDto);
});


app.MapPut("/api/v1/theater-chains/{chainId}/movies/{id}", async
    (ITheaterChainDtoMapperService mapperService, IRepository<ITheaterChain> theaterChainRepository, CancellationToken cancellationToken,
    int chainId, MovieDto moviDto) =>
{
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
    if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

    // Check if the movie exists in the theater chain
    IMovie existingMovie = theaterChain.GetMovieById(moviDto.Id);
    if (existingMovie is null) return Results.NotFound($"Movie[{moviDto?.Id}] not found in the theater chain[{chainId}].");

    // Update the movie with the new details from the movieDto
    theaterChain.UpdateMovie(existingMovie.Id, moviDto.Title, moviDto.Description, moviDto.Genre, moviDto.DurationMins, moviDto.ReleaseDateUtc);

    // Save changes to the repository
    await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

    // Optionally map the updated movie back to a DTO to return in the response
    MovieDto updatedMovieDto = mapperService.MapMovieToMovieDto(existingMovie);

    return Results.Ok(updatedMovieDto);
});


app.MapPut("/api/v1/theater-chains/{chainId}/movies/{id}/no-longer-available", async
    (IRepository<ITheaterChain> theaterChainRepository, ITheaterChainDtoMapperService mapperService,
    int chainId, int id, CancellationToken cancellationToken) =>
{
    // Retrieve the TheaterChain by ID
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
    if (theaterChain is null) return Results.NotFound($"Theater chain[{chainId}] not found.");

    // Check if the movie exists in the theater chain
    IMovie? existingMovie = theaterChain.GetMovieById(id);
    if (existingMovie is null) return Results.NotFound($"Movie[{id}] not found in theater chain[{chainId}].");

    // Mark the movie as no longer available
    theaterChain.MarkMovieAsNoLongerAvailable(id);

    // Save changes to the repository
    await theaterChainRepository.UpdateAsync(theaterChain, cancellationToken);

    IMovie movieUpdated = theaterChain.GetMovieById(id);
    MovieDto movieUpdatedDto = mapperService.MapMovieToMovieDto(movieUpdated);

    return Results.Ok(movieUpdatedDto);
});

//app.MapPut("/api/v1/theater-chains/{chainId}/movies/{id}/available", (MovieService movieService, int chainId, int id) =>
//{
//    // Mark as available logic
//});

app.MapGet("/api/v1/theater-chains/{chainId}/movies/{id}", async
    (ITheaterChainDtoMapperService mapperService, IRepository<ITheaterChain> theaterChainRepository, CancellationToken cancellationToken,
    int chainId, int movieId) =>
{
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);
    if (theaterChain is null)
    {
        return Results.NotFound();
    }

    IMovie movie = theaterChain.GetMovieById(movieId);
    if (movie is null)
    {
        return Results.NotFound();
    }

    MovieDto movieDto = mapperService.MapMovieToMovieDto(movie);
    return Results.Ok(movieDto);
});


app.MapGet("/api/v1/theater-chains/{chainId}/movies", async
    (ITheaterChainDtoMapperService mapperService, IRepository<ITheaterChain> theaterChainRepository, int chainId, CancellationToken cancellationToken)
    =>
{
    ITheaterChain? theaterChain = await theaterChainRepository.GetByIdAsync(chainId, cancellationToken);

    if (theaterChain is null)
    {
        return Results.NotFound();
    }

    List<IMovie> movies = theaterChain.GetMovies();
    IEnumerable<MovieDto> moviesDto = mapperService.MapMoviesToMoviesDto(movies);

    return Results.Ok(moviesDto);
});

//app.MapDelete("/api/v1/theater-chains/{chainId}/movies/{id}", (MovieService movieService, int chainId, int id) =>
//{
//    // Delete movie logic
//});

#endregion TheaterChain


app.Run();
