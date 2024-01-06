using Api;
using Api.Services;
using CinemaBooking.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


string[] summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    //    throw new MovieException("Mooov excpt!");

    int[] forecast = Enumerable.Range(1, 5).Select(index =>
            Random.Shared.Next(-20, 55)
                    )
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

TheaterChain _theaterChain = new(1, "Chain A", "Description A", []);
_theaterChain.AddMovie("Movie A", "Description A", "Genre A", TimeSpan.FromMinutes(120), DateTime.UtcNow);

app.MapPost("/api/v1/theater-chains/{chainId}/movies", (int chainId, Movie movieToAdd) =>
{
    Movie movieAdded = _theaterChain.AddMovie(movieToAdd.Title, movieToAdd.Description, movieToAdd.Genre, movieToAdd.Duration, movieToAdd.ReleaseDateUtc);

    return Results.Created($"/api/v1/theater-chains/{chainId}/movies/{movieAdded.Id}", movieAdded);
})
    .WithOpenApi();

//app.MapPost("/api/v1/theater-chains/{chainId}/movies/add", (MovieService movieService, int chainId, string title, string description, string genre, TimeSpan duration, DateTime releaseDateUtc) =>
//{
//    movieService.AddMovie(chainId, title, description, genre, duration, releaseDateUtc);
//    // Return appropriate response
//});

//app.MapPut("/api/v1/theater-chains/{chainId}/movies/{id}", (MovieService movieService, int chainId, int id, string title, string description, string genre, TimeSpan duration, DateTime releaseDateUtc) =>
//{
//    // Update movie logic
//});

//app.MapPut("/api/v1/theater-chains/{chainId}/movies/{id}/no-longer-available", (MovieService movieService, int chainId, int id) =>
//{
//    // Mark as no longer available logic
//});

//app.MapPut("/api/v1/theater-chains/{chainId}/movies/{id}/available", (MovieService movieService, int chainId, int id) =>
//{
//    // Mark as available logic
//});

//app.MapGet("/api/v1/theater-chains/{chainId}/movies/{id}", (int chainId, int id) =>
//{
//    return _theaterChain.GetMovieBy
//});

app.MapGet("/api/v1/theater-chains/{chainId}/movies", (ITheaterChainDtoMapperService mapperService, int chainId) =>
{
    List<Movie> movies = _theaterChain.GetMovies();

    return mapperService.MapMovieToMovideDto(movies);
});

//app.MapDelete("/api/v1/theater-chains/{chainId}/movies/{id}", (MovieService movieService, int chainId, int id) =>
//{
//    // Delete movie logic
//});


app.Run();
